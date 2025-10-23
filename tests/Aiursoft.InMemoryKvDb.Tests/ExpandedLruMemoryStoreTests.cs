using Aiursoft.InMemoryKvDb.AutoCreate;
using Aiursoft.InMemoryKvDb.ManualCreate;

namespace Aiursoft.InMemoryKvDb.Tests;

[TestClass]
public class ExpandedLruMemoryStoreTests
{
    [TestMethod]
    public void Test_LruMemoryStore_AutoCreate_GetAndSetIndexer()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 2);
        var id = Guid.NewGuid();

        // Act
        var player1 = store[id];
        store[id] = new Player(id, "NewPlayer");

        // Assert
        Assert.AreEqual("Player-" + id, player1.NickName);
        Assert.AreEqual("NewPlayer", store[id].NickName);
    }

    [TestMethod]
    public void Test_LruMemoryStore_AutoCreate_GetAll()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 3);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        store.GetOrAdd(id1);
        store.GetOrAdd(id2);

        // Act
        var allItems = store.GetAll().ToList();

        // Assert
        Assert.HasCount(2, allItems);
        Assert.IsTrue(allItems.Any(p => p.NickName == "Player-" + id1));
        Assert.IsTrue(allItems.Any(p => p.NickName == "Player-" + id2));
    }

    [TestMethod]
    public void Test_LruMemoryStore_AutoCreate_GetAllWithKeys()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 3);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        store.GetOrAdd(id1);
        store.GetOrAdd(id2);

        // Act
        var allItemsWithKeys = store.GetAllWithKeys().ToList();

        // Assert
        Assert.HasCount(2, allItemsWithKeys);
        Assert.IsTrue(allItemsWithKeys.Any(kv => kv.Key == id1));
        Assert.IsTrue(allItemsWithKeys.Any(kv => kv.Key == id2));
    }

    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_BasicAddAndEviction()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(2);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        // Act
        store.GetOrAdd(id1, id => new Player(id, $"Player-{id}"));
        store.GetOrAdd(id2, id => new Player(id, $"Player-{id}"));
        store.GetOrAdd(id3, id => new Player(id, $"Player-{id}")); // Evicts id1

        // Assert
        Assert.IsNull(store.Get(id1));
        Assert.IsNotNull(store.Get(id2));
        Assert.IsNotNull(store.Get(id3));
    }

    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_AddToCache()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(2);
        var id = Guid.NewGuid();
        var player = new Player(id, "ManuallyAdded");

        // Act
        store.AddToCache(id, player);

        // Assert
        Assert.AreEqual(player, store.Get(id));
    }

    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_GetAll()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(2);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        store.GetOrAdd(id1, id => new Player(id, "Player1"));
        store.GetOrAdd(id2, id => new Player(id, "Player2"));

        // Act
        var allItems = store.GetAll().ToList();

        // Assert
        Assert.HasCount(2, allItems);
        Assert.IsTrue(allItems.Any(p => p.NickName == "Player1"));
        Assert.IsTrue(allItems.Any(p => p.NickName == "Player2"));
    }

    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_GetAllWithKeys()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(2);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        store.GetOrAdd(id1, id => new Player(id, "Player1"));
        store.GetOrAdd(id2, id => new Player(id, "Player2"));

        // Act
        var allItemsWithKeys = store.GetAllWithKeys().ToList();

        // Assert
        Assert.HasCount(2, allItemsWithKeys);
        Assert.IsTrue(allItemsWithKeys.Any(kv => kv.Key == id1));
        Assert.IsTrue(allItemsWithKeys.Any(kv => kv.Key == id2));
    }
}
