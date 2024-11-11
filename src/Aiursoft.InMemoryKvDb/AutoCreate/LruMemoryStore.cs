using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb.AutoCreate;

public class LruMemoryStore<T, TK>(Func<TK, T> onNotFound, int maxCachedItemsCount) where TK : notnull
{
    private readonly Func<TK, T> _onNotFound = onNotFound ?? throw new ArgumentNullException(nameof(onNotFound));
    private readonly ConcurrentDictionary<TK, T> _store = new();
    private readonly LinkedList<TK> _lruList = new();
    private readonly object _lruLock = new();

    public T this[TK id]
    {
        get => GetOrAdd(id);
        set => AddToCache(id, value);
    }

    public T GetOrAdd(TK id)
    {
        if (_store.TryGetValue(id, out var value))
        {
            UpdateLru(id);
            return value;
        }

        value = _onNotFound(id);
        AddToCache(id, value);
        return value;
    }
    
    public T? Get(TK id)
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
    
    public IEnumerable<KeyValuePair<TK, T>> GetAllWithKeys()
    {
        lock (_lruLock)
        {
            return _store.ToList();
        }
    }

    private void UpdateLru(TK id)
    {
        lock (_lruLock)
        {
            _lruList.Remove(id);
            _lruList.AddLast(id);
        }
    }

    private void AddToCache(TK id, T value)
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
    
    public void Remove(TK id)
    {
        lock (_lruLock)
        {
            _store.TryRemove(id, out _);
            _lruList.Remove(id);
        }
    }
}