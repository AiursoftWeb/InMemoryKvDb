using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb.ManualCreate;

public class NamedLruMemoryStoreManualCreatedProvider<T, TK>(int maxCachedItemsCount) where TK : notnull
{
    private readonly ConcurrentDictionary<string, LruMemoryStoreManualCreated<T, TK>> _namedStores = new();

    public LruMemoryStoreManualCreated<T, TK> GetStore(string dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("DbName cannot be null or whitespace.", nameof(dbName));

        return _namedStores.GetOrAdd(dbName, _ => new LruMemoryStoreManualCreated<T, TK>(maxCachedItemsCount));
    }
}