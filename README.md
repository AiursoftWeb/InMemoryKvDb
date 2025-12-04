# Aiursoft InMemoryKvDb

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.com/aiursoft/inmemorykvdb/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.com/aiursoft/inmemorykvdb/badges/master/pipeline.svg)](https://gitlab.aiursoft.com/aiursoft/inmemorykvdb/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.com/aiursoft/inmemorykvdb/badges/master/coverage.svg)](https://gitlab.aiursoft.com/aiursoft/inmemorykvdb/-/pipelines)
[![NuGet version (Aiursoft.InMemoryKvDb)](https://img.shields.io/nuget/v/Aiursoft.inmemorykvdb.svg)](https://www.nuget.org/packages/Aiursoft.inmemorykvdb/)
[![Man hours](https://manhours.aiursoft.com/r/gitlab.aiursoft.com/aiursoft/inmemorykvdb.svg)](https://manhours.aiursoft.com/r/gitlab.aiursoft.com/aiursoft/inmemorykvdb.html)

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

## How to install

First, install `Aiursoft.InMemoryKvDb` to your ASP.NET Core project from [nuget.org](https://www.nuget.org/packages/Aiursoft.inmemorykvdb/):

```bash
dotnet add package Aiursoft.InMemoryKvDb
```

Add the service to your [`IServiceCollection`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) in `Startup.cs` (or `Program.cs` in .NET 6 and later):

## Usage Methods

1. `AddNamedLruMemoryStore<T, TK>`
2. `AddNamedLruMemoryStoreManualCreate<T, TK>`
3. `AddLruMemoryStore<T, TK>`
4. `AddLruMemoryStoreManualCreate<T, TK>`

### Differences and How to Choose

- **Named vs. Non-Named Stores**:
    - **Named stores** (`AddNamedLruMemoryStore` and `AddNamedLruMemoryStoreManualCreate`) allow you to create multiple isolated caches identified by unique names, suitable for scenarios where you need separate caches within the same application.
    - **Non-named stores** (`AddLruMemoryStore` and `AddLruMemoryStoreManualCreate`) are simpler and provide a single cache instance.

- **Automatic vs. Manual Key-Value Creation**:
    - **Automatic creation** (`AddLruMemoryStore` and `AddNamedLruMemoryStore`) uses an `onNotFound` function to generate values for missing keys automatically.
    - **Manual creation** (`AddLruMemoryStoreManualCreate` and `AddNamedLruMemoryStoreManualCreate`) requires explicit value creation when adding items, providing greater control over how values are added to the cache.

### Example Usages

#### 1. Using `AddNamedLruMemoryStore<T, TK>`

```csharp
var services = new ServiceCollection();
services.AddNamedLruMemoryStore<Player, Guid>(
    id => new Player(id, "GeneratedPlayer"),
    maxCachedItemsCount: 5);
var serviceProvider = services.BuildServiceProvider();
var namedProvider = serviceProvider.GetService<NamedLruMemoryStoreProvider<Player, Guid>>();
var store = namedProvider.GetStore("player-store");
var player = store.GetOrAdd(Guid.NewGuid());
```

#### 2. Using `AddNamedLruMemoryStoreManualCreate<T, TK>`

```csharp
var services = new ServiceCollection();
services.AddNamedLruMemoryStoreManualCreate<Player, Guid>(maxCachedItemsCount: 5);
var serviceProvider = services.BuildServiceProvider();
var namedProvider = serviceProvider.GetService<NamedLruMemoryStoreManualCreatedProvider<Player, Guid>>();
var store = namedProvider.GetStore("player-store");
var player = store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "ManualPlayer"));
```

#### 3. Using `AddLruMemoryStore<T, TK>`

```csharp
var services = new ServiceCollection();
services.AddLruMemoryStore<Player, Guid>(
    id => new Player(id, "GeneratedPlayer"),
    maxCachedItemsCount: 3);
var serviceProvider = services.BuildServiceProvider();
var store = serviceProvider.GetService<LruMemoryStore<Player, Guid>>();
var player = store.GetOrAdd(Guid.NewGuid());
```

#### 4. Using `AddLruMemoryStoreManualCreate<T, TK>`

```csharp
var services = new ServiceCollection();
services.AddLruMemoryStoreManualCreate<Player, Guid>(maxCachedItemsCount: 3);
var serviceProvider = services.BuildServiceProvider();
var store = serviceProvider.GetService<LruMemoryStoreManualCreated<Player, Guid>>();
var player = store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "ManualPlayer"));
```

Choose the appropriate method based on whether you need isolated named stores and whether you prefer automatic or manual value creation.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any feature requests, bug fixes, or other improvements.
