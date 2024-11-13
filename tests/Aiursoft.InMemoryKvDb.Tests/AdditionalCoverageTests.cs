using Aiursoft.InMemoryKvDb.AutoCreate;
using Aiursoft.InMemoryKvDb.ManualCreate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.InMemoryKvDb.Tests;

[TestClass]
public class AdditionalCoverageTests
{
    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_GetOrAdd_CacheHit()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(3);
        var id = Guid.NewGuid();
        var player = new Player(id, "CachedPlayer");

        // Act
        store.AddToCache(id, player);
        var retrievedPlayer = store.GetOrAdd(id, _ => new Player(id, "NewPlayer"));

        // Assert
        Assert.AreEqual("CachedPlayer", retrievedPlayer.NickName, "GetOrAdd should return the cached value when it exists.");
    }

    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_GetOrAdd_CacheMiss()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(3);
        var id = Guid.NewGuid();

        // Act
        var retrievedPlayer = store.GetOrAdd(id, _ => new Player(id, "NewPlayer"));

        // Assert
        Assert.AreEqual("NewPlayer", retrievedPlayer.NickName, "GetOrAdd should add and return the value if it is not in cache.");
    }

    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_Get_CacheHit()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(3);
        var id = Guid.NewGuid();
        var player = new Player(id, "CachedPlayer");
        store.AddToCache(id, player);

        // Act
        var retrievedPlayer = store.Get(id);

        // Assert
        Assert.IsNotNull(retrievedPlayer, "Get should return the cached value if it exists.");
        Assert.AreEqual("CachedPlayer", retrievedPlayer.NickName);
    }

    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_Get_CacheMiss()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(3);
        var id = Guid.NewGuid();

        // Act
        var retrievedPlayer = store.Get(id);

        // Assert
        Assert.IsNull(retrievedPlayer, "Get should return null (default) if the value is not in cache.");
    }

    [TestMethod]
    public void Test_LruMemoryStore_AutoCreate_GetOrAdd_CacheHit()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 3);
        var id = Guid.NewGuid();
        var player = new Player(id, "CachedPlayer");
        store[id] = player; // Using indexer to add

        // Act
        var retrievedPlayer = store.GetOrAdd(id);

        // Assert
        Assert.AreEqual("CachedPlayer", retrievedPlayer.NickName, "GetOrAdd should return the cached value when it exists.");
    }

    [TestMethod]
    public void Test_LruMemoryStore_AutoCreate_GetOrAdd_CacheMiss()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 3);
        var id = Guid.NewGuid();

        // Act
        var retrievedPlayer = store.GetOrAdd(id);

        // Assert
        Assert.AreEqual($"Player-{id}", retrievedPlayer.NickName, "GetOrAdd should create and add the value if it is not in cache.");
    }

    [TestMethod]
    public void Test_LruMemoryStore_AutoCreate_Get_CacheHit()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 3);
        var id = Guid.NewGuid();
        var player = new Player(id, "CachedPlayer");
        store[id] = player; // Using indexer to add

        // Act
        var retrievedPlayer = store.Get(id);

        // Assert
        Assert.IsNotNull(retrievedPlayer, "Get should return the cached value if it exists.");
        Assert.AreEqual("CachedPlayer", retrievedPlayer.NickName);
    }

    [TestMethod]
    public void Test_LruMemoryStore_AutoCreate_Get_CacheMiss()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 3);
        var id = Guid.NewGuid();

        // Act
        var retrievedPlayer = store.Get(id);

        // Assert
        Assert.IsNull(retrievedPlayer, "Get should return null (default) if the value is not in cache.");
    }
    
    [TestMethod]
    public void Test_LruMemoryStoreManualCreated_Remove_ExistingKey()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(3);
        var id = Guid.NewGuid();
        var player = new Player(id, "PlayerToRemove");
        store.AddToCache(id, player);

        // Act
        store.Remove(id);
        var retrievedPlayer = store.Get(id);

        // Assert
        Assert.IsNull(retrievedPlayer, "Remove should delete the item from the cache.");
    }

    [TestMethod]
    public void Test_LruMemoryStore_Remove_ExistingKey()
    {
        // Arrange
        var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 3);
        var id = Guid.NewGuid();
        var player = new Player(id, "PlayerToRemove");
        store[id] = player; // Using indexer to add

        // Act
        store.Remove(id);
        var retrievedPlayer = store.Get(id);

        // Assert
        Assert.IsNull(retrievedPlayer, "Remove should delete the item from the cache.");
    }
}