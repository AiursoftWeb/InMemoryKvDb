
using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb.AutoCreate;

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