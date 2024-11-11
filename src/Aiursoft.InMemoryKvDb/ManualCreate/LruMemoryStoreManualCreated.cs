using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb.ManualCreate;

public class LruMemoryStoreManualCreated<T, TK>(int maxCachedItemsCount) where TK : notnull
{
    private readonly ConcurrentDictionary<TK, T> _store = new();
    private readonly LinkedList<TK> _lruList = new();
    private readonly object _lruLock = new();

    public T GetOrAdd(TK id, Func<TK, T> onNotFound)
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

    public T? Get(TK id)
    {
        if (_store.TryGetValue(id, out var value))
        {
            UpdateLru(id);
            return value;
        }
        
        return default;
    }
    
    public IEnumerable<T> GetAll()
    {
        lock (_lruLock)
        {
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

    public void AddToCache(TK id, T value)
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