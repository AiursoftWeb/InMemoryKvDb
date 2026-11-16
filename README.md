# Aiursoft InMemoryKvDb

<a name="aiursoft-inmemorykvdb"></a>

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AiursoftWeb/inmemorykvdb/blob/master/LICENSE)
[![Pipeline stat](https://badge-proxy.aiursoft.com/aiursoft/inmemorykvdb/badges/master/pipeline.svg)](https://gitlab.aiursoft.com/aiursoft/inmemorykvdb/-/pipelines)
[![Test Coverage](https://badge-proxy.aiursoft.com/aiursoft/inmemorykvdb/badges/master/coverage.svg)](https://gitlab.aiursoft.com/aiursoft/inmemorykvdb/-/pipelines)
[![S360 compliance](https://s360.aiursoft.com/badge.svg?project=aiursoft%2Finmemorykvdb)](https://s360.aiursoft.com/SupplyChain?tab=projects&project=aiursoft%2Finmemorykvdb&scope=baseline&ecosystem=all)
[![NuGet version (Aiursoft.InMemoryKvDb)](https://img.shields.io/nuget/v/Aiursoft.inmemorykvdb.svg)](https://www.nuget.org/packages/Aiursoft.inmemorykvdb/)
[![Man hours](https://manhours.aiursoft.com/r/github.com/aiursoftweb/inmemorykvdb.svg)](https://manhours.aiursoft.com/r/github.com/aiursoftweb/inmemorykvdb.html)

Aiursoft InMemoryKvDb is a lightweight, thread-safe, in-memory key-value database with support for automatic Least Recently Used (LRU) cache eviction. It allows for efficient storage and retrieval of key-value pairs while preventing unbounded memory usage by leveraging configurable LRU eviction policies.

This library is ideal for scenarios where you need an in-memory cache that automatically discards the least recently accessed items, such as storing temporary user data, caching API responses, or handling in-memory operations for temporary data processing, without worrying about potential memory overflows.

## Overview

### What is InMemoryKvDb?

<a name="why-this-project"></a>

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

Add the package to your .NET project:

<a name="how-to-install"></a>

First, install `Aiursoft.InMemoryKvDb` to your ASP.NET Core project from [nuget.org](https://www.nuget.org/packages/Aiursoft.inmemorykvdb/):

```bash
dotnet add package Aiursoft.InMemoryKvDb
```

Add the service to your [`IServiceCollection`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) in `Startup.cs` (or `Program.cs` in .NET 6 and later):

**Standalone console example**

The console example also uses the standard Microsoft service registrations:

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
```

Requires .NET 10 or later.

## Usage

Start with this complete console `Program.cs` after installing the package:

```csharp
using Aiursoft.InMemoryKvDb;
using Aiursoft.InMemoryKvDb.AutoCreate;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLruMemoryStore<string, Guid>(id => $"Player {id}", maxCachedItemsCount: 3);
using var provider = services.BuildServiceProvider();
var store = provider.GetRequiredService<LruMemoryStore<string, Guid>>();
Console.WriteLine(store.GetOrAdd(Guid.Empty));
```

<a name="usage-methods"></a>

1. `AddNamedLruMemoryStore<T, TK>`
2. `AddNamedLruMemoryStoreManualCreate<T, TK>`
3. `AddLruMemoryStore<T, TK>`
4. `AddLruMemoryStoreManualCreate<T, TK>`

### Differences and how to choose

<a name="differences-and-how-to-choose"></a>

- **Named vs. Non-Named Stores**:
    - **Named stores** (`AddNamedLruMemoryStore` and `AddNamedLruMemoryStoreManualCreate`) allow you to create multiple isolated caches identified by unique names, suitable for scenarios where you need separate caches within the same application.
    - **Non-named stores** (`AddLruMemoryStore` and `AddLruMemoryStoreManualCreate`) are simpler and provide a single cache instance.

- **Automatic vs. Manual Key-Value Creation**:
    - **Automatic creation** (`AddLruMemoryStore` and `AddNamedLruMemoryStore`) uses an `onNotFound` function to generate values for missing keys automatically.
    - **Manual creation** (`AddLruMemoryStoreManualCreate` and `AddNamedLruMemoryStoreManualCreate`) requires explicit value creation when adding items, providing greater control over how values are added to the cache.

### Example usages

<a name="example-usages"></a>

#### 1. Using `AddNamedLruMemoryStore<T, TK>`

<a name="1-using-addnamedlrumemorystoret-tk"></a>

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

<a name="2-using-addnamedlrumemorystoremanualcreatet-tk"></a>

```csharp
var services = new ServiceCollection();
services.AddNamedLruMemoryStoreManualCreate<Player, Guid>(maxCachedItemsCount: 5);
var serviceProvider = services.BuildServiceProvider();
var namedProvider = serviceProvider.GetService<NamedLruMemoryStoreManualCreatedProvider<Player, Guid>>();
var store = namedProvider.GetStore("player-store");
var player = store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "ManualPlayer"));
```

#### 3. Using `AddLruMemoryStore<T, TK>`

<a name="3-using-addlrumemorystoret-tk"></a>

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

<a name="4-using-addlrumemorystoremanualcreatet-tk"></a>

```csharp
var services = new ServiceCollection();
services.AddLruMemoryStoreManualCreate<Player, Guid>(maxCachedItemsCount: 3);
var serviceProvider = services.BuildServiceProvider();
var store = serviceProvider.GetService<LruMemoryStoreManualCreated<Player, Guid>>();
var player = store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "ManualPlayer"));
```

Choose the appropriate method based on whether you need isolated named stores and whether you prefer automatic or manual value creation.

## Configuration

**Capacity and lifetime**

The four registration methods default `maxCachedItemsCount` to `1024`; the examples use smaller explicit capacities. They register singleton store or provider instances. Named stores separate caches by name, while automatic factories and manual creation decide how a missing value is produced. Data exists only in the application's memory and is not persisted across process restarts. Configure capacity and value factories in code; no external configuration file is required.

## Development

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

From the repository root, restore, build and test:

```bash
dotnet restore Aiursoft.InMemoryKvDb.sln
dotnet build Aiursoft.InMemoryKvDb.sln --no-restore
dotnet test Aiursoft.InMemoryKvDb.sln --no-restore
```

Run `./lint.sh` for the existing static checks.

## How to contribute

<a name="contributing"></a>

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you have push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.

## License

Licensed under the [MIT License](https://github.com/AiursoftWeb/inmemorykvdb/blob/master/LICENSE).
