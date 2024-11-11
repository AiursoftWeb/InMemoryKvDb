
using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb;

public class NamedLruMemoryStoreProvider<T>(Func<Guid, T> onNotFound, int maxCachedItemsCount)
{
    private readonly ConcurrentDictionary<string, LruMemoryStore<T>> _namedStores = new();

    public LruMemoryStore<T> GetStore(string dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("DbName cannot be null or whitespace.", nameof(dbName));

        return _namedStores.GetOrAdd(dbName, _ => new LruMemoryStore<T>(onNotFound, maxCachedItemsCount));
    }
}

// Extension method for service registration

// Example usage:
// Registering the service
// services.AddNamedLruMemoryStore<Player>(id => new Player(id) { NickName = "Anonymous " + new Random().Next(1000, 9999) }, 4096, 128);

// Accessing a specific store by name
// var namedProvider = serviceProvider.GetRequiredService<NamedLruMemoryStoreProvider<Player>>();
// var normalPlayerDb = namedProvider.GetStore("NormalPlayerDb");
// var highLevelPlayerDb = namedProvider.GetStore("HighLevelPlayerDb");
