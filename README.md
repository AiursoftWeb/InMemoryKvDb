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

#### 1. Registering the Service

In your ASP.NET Core application, register the `LruMemoryStore` service with your desired configuration in `Startup.cs` (or `Program.cs` for .NET 6+):

```csharp
public class Player
{
    public Guid Id { get; set; }
    public string NickName { get; set; }
    
    public Player(Guid id)
    {
        Id = id;
    }
}

services.AddNamedLruMemoryStore<Player>(
    id => new Player(id) { NickName = "Anonymous " + new Random().Next(1000, 9999) },
    maxCachedItemsCount: 4096);
```

This configuration sets up a store with a maximum of 4096 cached items and uses a custom initialization function to generate a `Player` instance when a key is not found.

#### 2. Accessing the Store

To use a specific named store, retrieve an instance of `NamedLruMemoryStoreProvider<T>` and access the desired named store:

```csharp
var namedProvider = serviceProvider.GetRequiredService<NamedLruMemoryStoreProvider<Player>>();
var normalPlayerDb = namedProvider.GetStore("NormalPlayerDb");
var highLevelPlayerDb = namedProvider.GetStore("HighLevelPlayerDb");

// Retrieve or add a player to the normal player database
var playerId = Guid.NewGuid();
var player = normalPlayerDb.GetOrAdd(playerId);
```

#### 3. LRU Eviction in Action

The `LruMemoryStore` automatically manages the eviction of least recently used items when the maximum capacity is reached. You can set different capacities for different named stores as needed.

### Configuration Options

- `maxCachedItemsCount`: The maximum number of items allowed in the store before eviction starts. Default is 4096.

### Example Scenario

Consider a multiplayer game where player data is cached in memory for fast access. Using `Aiursoft.InMemoryKvDb`, you can efficiently cache player profiles while automatically managing memory usage:

```csharp
services.AddNamedLruMemoryStore<Player>(
    id => new Player(id) { NickName = "Guest" + new Random().Next(1000, 9999) },
    maxCachedItemsCount: 2048);
```

By using `AddNamedLruMemoryStore`, you ensure that memory usage remains bounded, reducing the risk of `OutOfMemoryException` from uncontrolled cache growth.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any feature requests, bug fixes, or other improvements.
