using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb.ManualCreate;

public class LruMemoryStoreManualCreated<T>(int maxCachedItemsCount)
{
    private readonly ConcurrentDictionary<Guid, T> _store = new();
    private readonly LinkedList<Guid> _lruList = new();
    private readonly object _lruLock = new();

    public T GetOrAdd(Guid id, Func<Guid, T> onNotFound)
    {
        if (_store.TryGetValue(id, out var value))
        {
            UpdateLru(id);
            return value;
        }

        value = onNotFound(id);
        AddToCache(id, value);
        return value;
    }

    public T? Get(Guid id)
    {
        if (_store.TryGetValue(id, out var value))
        {
            UpdateLru(id);
            return value;
        }
        
        return default;
    }
    
    // 获取所有元素
    public IEnumerable<T> GetAll()
    {
        lock (_lruLock)
        {
            // 返回所有元素的副本，以支持 LINQ 查询而不影响 LRU 逻辑
            return _store.Values.ToList();
        }
    }

    private void UpdateLru(Guid id)
    {
        lock (_lruLock)
        {
            _lruList.Remove(id);
            _lruList.AddLast(id);
        }
    }

    public void AddToCache(Guid id, T value)
    {
        lock (_lruLock)
        {
            if (_store.Count >= maxCachedItemsCount && _lruList.Count > 0)
            {
                var oldestItem = _lruList.First!.Value;
                _lruList.RemoveFirst();
                _store.TryRemove(oldestItem, out _);
            }

            _store[id] = value;
            _lruList.AddLast(id);
        }
    }
}