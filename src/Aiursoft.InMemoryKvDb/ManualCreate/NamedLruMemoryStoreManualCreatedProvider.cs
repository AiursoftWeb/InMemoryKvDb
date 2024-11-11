using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb.ManualCreate;

public class NamedLruMemoryStoreManualCreatedProvider<T>(int maxCachedItemsCount)
{
    private readonly ConcurrentDictionary<string, LruMemoryStoreManualCreated<T>> _namedStores = new();

    public LruMemoryStoreManualCreated<T> GetStore(string dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("DbName cannot be null or whitespace.", nameof(dbName));

        return _namedStores.GetOrAdd(dbName, _ => new LruMemoryStoreManualCreated<T>(maxCachedItemsCount));
    }
}