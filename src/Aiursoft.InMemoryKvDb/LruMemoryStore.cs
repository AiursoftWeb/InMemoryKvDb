using System.Collections.Concurrent;

namespace Aiursoft.InMemoryKvDb;

public class LruMemoryStore<T>(Func<Guid, T> onNotFound, int maxCachedItemsCount)
{
    private readonly Func<Guid, T> _onNotFound = onNotFound ?? throw new ArgumentNullException(nameof(onNotFound));
    private readonly ConcurrentDictionary<Guid, T> _store = new();
    private readonly LinkedList<Guid> _lruList = new();
    private readonly object _lruLock = new();

    public T GetOrAdd(Guid id)
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

    private void UpdateLru(Guid id)
    {
        lock (_lruLock)
        {
            _lruList.Remove(id);
            _lruList.AddLast(id);
        }
    }

    private void AddToCache(Guid id, T value)
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