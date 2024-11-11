using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.InMemoryKvDb.Tests;

[TestClass]
public class NamedLruMemoryStoreProviderTests
{
    private readonly NamedLruMemoryStoreProvider<Player> _namedProvider;

    public NamedLruMemoryStoreProviderTests()
    {
        _namedProvider = new NamedLruMemoryStoreProvider<Player>(
            id => new Player(id, "TestPlayer" + id),
            maxCachedItemsCount: 3);
    }

    [TestMethod]
    public void GetStore_ReturnsSameInstanceForSameName()
    {
        var store1 = _namedProvider.GetStore("TestDb");
        var store2 = _namedProvider.GetStore("TestDb");

        Assert.AreSame(store1, store2, "Stores retrieved with the same name should be the same instance.");
    }

    [TestMethod]
    public void GetStore_ReturnsDifferentInstancesForDifferentNames()
    {
        var store1 = _namedProvider.GetStore("TestDb1");
        var store2 = _namedProvider.GetStore("TestDb2");

        Assert.AreNotSame(store1, store2, "Stores retrieved with different names should be different instances.");
    }

    [TestMethod]
    public void LruMemoryStore_GetOrAdd_AddsNewItem()
    {
        var store = _namedProvider.GetStore("TestDb");
        var playerId = Guid.NewGuid();
        var player = store.GetOrAdd(playerId);

        Assert.IsNotNull(player, "Player should not be null.");
        Assert.AreEqual("TestPlayer" + playerId, player.NickName, "Player should have the correct NickName.");
    }

    [TestMethod]
    public void LruMemoryStore_GetOrAdd_RespectsLruEviction()
    {
        var store = _namedProvider.GetStore("TestDb");

        // Add multiple players to exceed maxCachedItemsCount
        var player1 = store.GetOrAdd(Guid.NewGuid());
        var player2 = store.GetOrAdd(Guid.NewGuid());
        _ = store.GetOrAdd(Guid.NewGuid());
        _ = store.GetOrAdd(Guid.NewGuid()); // This should trigger eviction of the oldest item

        Assert.AreEqual(3, GetStoreCount(store), "Store should contain the maximum number of allowed items.");

        // Access player2 to make it "hot"
        store.GetOrAdd(player2.Id);

        // Add another player to trigger another eviction
        _ = store.GetOrAdd(Guid.NewGuid());

        Assert.AreEqual(3, GetStoreCount(store), "Store should maintain the max cache size.");
        Assert.IsFalse(IsPlayerInStore(store, player1.Id), "Oldest player (player1) should have been evicted.");
    }

    [TestMethod]
    public void LruMemoryStore_Get_ReturnsNullIfItemNotFound()
    {
        var store = _namedProvider.GetStore("TestDb");
        var playerId = Guid.NewGuid();
        var player = store.Get(playerId);

        Assert.IsNull(player, "Expected null when attempting to get a non-existent player.");
    }

    [TestMethod]
    public void LruMemoryStore_GetAll_ReturnsAllCachedItems()
    {
        var store = _namedProvider.GetStore("TestDb");

        var player1 = store.GetOrAdd(Guid.NewGuid());
        var player2 = store.GetOrAdd(Guid.NewGuid());
        var player3 = store.GetOrAdd(Guid.NewGuid());

        var allItems = store.GetAll().ToList();

        Assert.AreEqual(3, allItems.Count, "Expected all cached items to be returned.");
        CollectionAssert.Contains(allItems, player1, "Expected player1 to be in the list.");
        CollectionAssert.Contains(allItems, player2, "Expected player2 to be in the list.");
        CollectionAssert.Contains(allItems, player3, "Expected player3 to be in the list.");
    }

    [TestMethod]
    public void LruMemoryStore_AddToCache_OverwritesExistingItem()
    {
        var store = _namedProvider.GetStore("TestDb");
        var playerId = Guid.NewGuid();
        _ = store[playerId];

        var player2 = new Player(playerId, "NewTestPlayer");
        store[playerId] = player2;

        var retrievedPlayer = store.Get(playerId);

        Assert.AreEqual("NewTestPlayer", retrievedPlayer!.NickName, "Expected the existing item to be overwritten.");
    }

    [TestMethod]
    public void LruMemoryStore_AddToCache_HandlesEvictionCorrectlyWhenAtMaxCapacity()
    {
        var store = _namedProvider.GetStore("TestDb");

        // Add players to fill up the cache
        var player1 = store[Guid.NewGuid()];
        store.GetOrAdd(Guid.NewGuid());
        store.GetOrAdd(Guid.NewGuid());

        // Add a new player to trigger eviction
        store.GetOrAdd(Guid.NewGuid());

        // Confirm the oldest player (player1) has been evicted
        Assert.IsFalse(IsPlayerInStore(store, player1.Id), "Oldest player (player1) should have been evicted.");
        Assert.AreEqual(3, GetStoreCount(store), "Store should maintain the max cache size.");
    }

    [TestMethod]
    public void AddNamedLruMemoryStore_RegistersProviderCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNamedLruMemoryStore<Player>(
            id => new Player(id, "TestPlayer"),
            maxCachedItemsCount: 3);

        var serviceProvider = services.BuildServiceProvider();
        var namedProvider = serviceProvider.GetService<NamedLruMemoryStoreProvider<Player>>();

        // Assert
        Assert.IsNotNull(namedProvider, "NamedLruMemoryStoreProvider should be registered and resolved.");

        // Additional behavior check
        var store = namedProvider.GetStore("TestDb");
        var player = store.GetOrAdd(Guid.NewGuid());

        Assert.IsNotNull(player, "Player instance should not be null when retrieved.");
        Assert.AreEqual("TestPlayer", player.NickName, "Player should have the correct NickName set by onNotFound.");
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [ExcludeFromCodeCoverage]
    public void GetStore_ThrowsExceptionForNullOrWhitespaceDbName()
    {
        var namedProvider = new NamedLruMemoryStoreProvider<Player>(
            id => new Player(id, "TestPlayer"),
            maxCachedItemsCount: 3);

        // Attempt to get a store with an invalid (null or whitespace) name
        namedProvider.GetStore(" ");
    }

    private int GetStoreCount(LruMemoryStore<Player> store)
    {
        // Access the private field using reflection for testing
        var field = typeof(LruMemoryStore<Player>).GetField("_store",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (System.Collections.Concurrent.ConcurrentDictionary<Guid, Player>)field!.GetValue(store)!;
        return dictionary.Count;
    }

    private bool IsPlayerInStore(LruMemoryStore<Player> store, Guid playerId)
    {
        var field = typeof(LruMemoryStore<Player>).GetField("_store",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (System.Collections.Concurrent.ConcurrentDictionary<Guid, Player>)field!.GetValue(store)!;
        return dictionary.ContainsKey(playerId);
    }


    // Basic Player class for testing purposes
    private class Player(Guid id, string nickName)
    {
        public Guid Id { get; } = id;
        public string NickName { get; } = nickName;
    }
}