using Aiursoft.InMemoryKvDb.ManualCreate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.InMemoryKvDb.Tests;

[TestClass]
public class LruMemoryStoreManualCreatedTests
{
    private readonly LruMemoryStoreManualCreated<Player> _store = new(maxCachedItemsCount: 3);

    [TestMethod]
    public void ManualStore_GetOrAdd_AddsNewItem()
    {
        var playerId = Guid.NewGuid();
        var player = _store.GetOrAdd(playerId, id => new Player(id, "TestPlayer"));

        Assert.IsNotNull(player, "Player should not be null.");
        Assert.AreEqual("TestPlayer", player.NickName, "Player should have the correct NickName.");
    }

    [TestMethod]
    public void ManualStore_GetOrAdd_RespectsLruEviction()
    {
        var player1 = _store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "Player1"));
        _ = _store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "Player2"));
        _store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "Player3"));
        _store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "Player4")); // This should trigger eviction of the oldest item

        Assert.AreEqual(3, GetStoreCount(_store), "Store should contain the maximum number of allowed items.");
        Assert.IsFalse(IsPlayerInStore(_store, player1.Id), "Oldest player (player1) should have been evicted.");
    }

    [TestMethod]
    public void ManualStore_Get_ReturnsNullIfItemNotFound()
    {
        var playerId = Guid.NewGuid();
        var player = _store.Get(playerId);

        Assert.IsNull(player, "Expected null when attempting to get a non-existent player.");
    }

    [TestMethod]
    public void ManualStore_GetAll_ReturnsAllCachedItems()
    {
        var player1 = _store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "Player1"));
        var player2 = _store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "Player2"));
        var player3 = _store.GetOrAdd(Guid.NewGuid(), id => new Player(id, "Player3"));

        var allItems = _store.GetAll().ToList();

        Assert.AreEqual(3, allItems.Count, "Expected all cached items to be returned.");
        CollectionAssert.Contains(allItems, player1, "Expected player1 to be in the list.");
        CollectionAssert.Contains(allItems, player2, "Expected player2 to be in the list.");
        CollectionAssert.Contains(allItems, player3, "Expected player3 to be in the list.");
    }

    [TestMethod]
    public void ManualStore_AddToCache_OverwritesExistingItem()
    {
        var playerId = Guid.NewGuid();
        _store.AddToCache(playerId, new Player(playerId, "Player1"));

        var player2 = new Player(playerId, "NewPlayer");
        _store.AddToCache(playerId, player2);

        var retrievedPlayer = _store.Get(playerId);

        Assert.AreEqual("NewPlayer", retrievedPlayer!.NickName, "Expected the existing item to be overwritten.");
    }

    [TestMethod]
    public void ManualStore_AddToCache_HandlesEvictionCorrectlyWhenAtMaxCapacity()
    {
        var player1 = new Player(Guid.NewGuid(), "Player1");
        _store.AddToCache(player1.Id, player1);
        _store.AddToCache(Guid.NewGuid(), new Player(Guid.NewGuid(), "Player2"));
        _store.AddToCache(Guid.NewGuid(), new Player(Guid.NewGuid(), "Player3"));

        // Add a new player to trigger eviction
        _store.AddToCache(Guid.NewGuid(), new Player(Guid.NewGuid(), "Player4"));

        Assert.IsFalse(IsPlayerInStore(_store, player1.Id), "Oldest player (player1) should have been evicted.");
        Assert.AreEqual(3, GetStoreCount(_store), "Store should maintain the max cache size.");
    }

    private int GetStoreCount(LruMemoryStoreManualCreated<Player> store)
    {
        // Access the private field using reflection for testing
        var field = typeof(LruMemoryStoreManualCreated<Player>).GetField("_store",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (System.Collections.Concurrent.ConcurrentDictionary<Guid, Player>)field!.GetValue(store)!;
        return dictionary.Count;
    }

    private bool IsPlayerInStore(LruMemoryStoreManualCreated<Player> store, Guid playerId)
    {
        var field = typeof(LruMemoryStoreManualCreated<Player>).GetField("_store",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (System.Collections.Concurrent.ConcurrentDictionary<Guid, Player>)field!.GetValue(store)!;
        return dictionary.ContainsKey(playerId);
    }
}