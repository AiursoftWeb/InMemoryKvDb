
using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb.AutoCreate;

public class NamedLruMemoryStoreProvider<T, TK>(Func<TK, T> onNotFound, int maxCachedItemsCount) where TK : notnull
{
    private readonly ConcurrentDictionary<string, LruMemoryStore<T, TK>> _namedStores = new();

    public LruMemoryStore<T, TK> GetStore(string dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("DbName cannot be null or whitespace.", nameof(dbName));

        return _namedStores.GetOrAdd(dbName, _ => new LruMemoryStore<T, TK>(onNotFound, maxCachedItemsCount));
    }
}