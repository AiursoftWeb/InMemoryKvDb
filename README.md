# Aiursoft InMemoryKvDb

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.cn/aiursoft/inmemorykvdb/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.cn/aiursoft/inmemorykvdb/badges/master/pipeline.svg)](https://gitlab.aiursoft.cn/aiursoft/inmemorykvdb/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.cn/aiursoft/inmemorykvdb/badges/master/coverage.svg)](https://gitlab.aiursoft.cn/aiursoft/inmemorykvdb/-/pipelines)
[![NuGet version (Aiursoft.InMemoryKvDb)](https://img.shields.io/nuget/v/Aiursoft.inmemorykvdb.svg)](https://www.nuget.org/packages/Aiursoft.inmemorykvdb/)
[![ManHours](https://manhours.aiursoft.cn/r/gitlab.aiursoft.cn/aiursoft/inmemorykvdb.svg)](https://gitlab.aiursoft.cn/aiursoft/inmemorykvdb/-/commits/master?ref_type=heads)

Aiursoft InMemoryKvDb is a lightweight, thread-safe, in-memory key-value database with support for automatic Least Recently Used (LRU) cache eviction. It allows for efficient storage and retrieval of key-value pairs while preventing unbounded memory usage by leveraging configurable LRU eviction policies.

This library is ideal for scenarios where you need an in-memory cache that automatically discards the least recently accessed items, such as storing temporary user data, caching API responses, or handling in-memory operations for temporary data processing, without worrying about potential memory overflows.

## Why this project

The traditional way to create an In-Memory Key-Value Database in C# is:

```csharp
public class InMemoryDatabase : ISingletonDependency
{
    private ConcurrentDictionary<Guid, Player> Players { get; } = new();
    
    public Player GetOrAddPlayer(Guid id)
    {
        lock (Players)
        {
            return Players.GetOrAdd(id, _ => new Player(id)
            {
                NickName = "Anonymous " + new Random().Next(1000, 9999)
            });
        }
    }
    
}
```

However, if a hacker keeps calling `GetOrAddPlayer` with a new `Guid`, the `Players` dictionary will grow infinitely. And cause you `OutOfMemoryException`.

## Installation

First, install `Aiursoft.InMemoryKvDb` to your ASP.NET Core project from [nuget.org](https://www.nuget.org/packages/Aiursoft.inmemorykvdb/):

```bash
dotnet add package Aiursoft.InMemoryKvDb
```

Add the service to your [`IServiceCollection`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) in `Startup.cs` (or `Program.cs` in .NET 6 and later):

### Registering and Using Aiursoft.InMemoryKvDb with Different Methods

#### 1. Service Registration Methods

`Aiursoft.InMemoryKvDb` offers four different registration methods to suit varying caching needs. Choose the one that fits your requirements:

##### 1.1 Using `AddNamedLruMemoryStore<T>`

This method registers a named `LruMemoryStore` provider, where each name is associated with a separate LRU store.

```csharp
services.AddNamedLruMemoryStore<Player>(
    id => new Player(id) { NickName = "Anonymous " + new Random().Next(1000, 9999) },
    maxCachedItemsCount: 4096);
```

- **`onNotFound`**: A custom creation function that generates a new item when it is not found.
- **`maxCachedItemsCount`**: Sets the maximum cache size. When this limit is exceeded, the LRU eviction mechanism is triggered.

##### 1.2 Using `AddNamedLruMemoryStoreManualCreate<T>`

This method also registers a named `LruMemoryStore` provider, but it does not automatically invoke `onNotFound` when a new item needs to be created. Creation logic is managed manually.

```csharp
services.AddNamedLruMemoryStoreManualCreate<Player>(maxCachedItemsCount: 2048);
```

- Suitable for scenarios where auto-creation is not required or item generation needs manual control.

##### 1.3 Using `AddLruMemoryStore<T>`

This method registers a regular `LruMemoryStore` that is not name-based, treating each store instance as a standalone entity.

```csharp
services.AddLruMemoryStore<Player>(
    id => new Player(id) { NickName = "Guest" + new Random().Next(1000, 9999) },
    maxCachedItemsCount: 1024);
```

- `LruMemoryStore` is suitable for simpler scenarios where named namespace management or switching between store instances is not necessary.

##### 1.4 Using `AddLruMemoryStoreManualCreate<T>`

This method is similar to `AddLruMemoryStore`, but it does not auto-create new items, requiring manual addition and management.

```csharp
services.AddLruMemoryStoreManualCreate<Player>(maxCachedItemsCount: 512);
```

- Best suited for flexible data item management or specific creation control requirements.

---

#### 2. Accessing the Store

##### 2.1 Accessing `NamedLruMemoryStoreProvider<T>`

When using `AddNamedLruMemoryStore` or `AddNamedLruMemoryStoreManualCreate`, you can access specific named stores through `NamedLruMemoryStoreProvider<T>`.

```csharp
var namedProvider = serviceProvider.GetRequiredService<NamedLruMemoryStoreProvider<Player>>();
var normalPlayerDb = namedProvider.GetStore("NormalPlayerDb");
var highLevelPlayerDb = namedProvider.GetStore("HighLevelPlayerDb");

// Retrieve or add a player
var playerId = Guid.NewGuid();
var player = normalPlayerDb.GetOrAdd(playerId);
```

##### 2.2 Accessing `LruMemoryStore<T>`

When using `AddLruMemoryStore` or `AddLruMemoryStoreManualCreate`, you can directly use the store instance.

```csharp
var store = serviceProvider.GetRequiredService<LruMemoryStore<Player>>();
var playerId = Guid.NewGuid();
var player = store.GetOrAdd(playerId);
```

---

#### 3. LRU Eviction Mechanism

Regardless of the chosen storage type, all instances follow the LRU (Least Recently Used) strategy. When the cache reaches its maximum capacity, the least recently used items will be automatically evicted to free up space.

#### 4. Configuration Options

- `maxCachedItemsCount`: Controls the maximum cache size, triggering eviction when this limit is exceeded.
- `onNotFound` (for auto-creating stores only): A custom function to generate new items when they are not found.

---

#### Example Scenario

```csharp
services.AddNamedLruMemoryStore<Player>(
    id => new Player(id) { NickName = "Guest" + new Random().Next(1000, 9999) },
    maxCachedItemsCount: 2048);
```

By using `AddNamedLruMemoryStore`, you can efficiently cache player data across different namespaces while ensuring controlled memory usage, reducing the risk of `OutOfMemoryException`.

---

### Choosing the Right Method

- **Auto-Create**: Ideal for automatically generating cached objects and reducing code complexity.
- **Manual-Create**: Suitable for scenarios requiring fine-grained control over data creation logic.
- **Named Stores**: Useful for distinguishing data storage across different contexts.
- **Non-Named Stores**: Best for simpler caching needs.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any feature requests, bug fixes, or other improvements.
