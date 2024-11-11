using Aiursoft.InMemoryKvDb;
using Aiursoft.InMemoryKvDb.ManualCreate;
using Aiursoft.InMemoryKvDb.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LruMemoryStoreManualCreatedTests
{
    [TestMethod]
    public void Test_LruMemoryStore_ManualCreate_BasicOperations()
    {
        // Arrange
        var store = new LruMemoryStoreManualCreated<Player, Guid>(2);

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        // Act
        var player1 = store.GetOrAdd(id1, id => new Player(id, $"Player-{id}"));
        var player2 = store.GetOrAdd(id2, id => new Player(id, $"Player-{id}"));
        var player3 = store.GetOrAdd(id3, id => new Player(id, $"Player-{id}")); // Should evict id1

        // Assert
        Assert.AreEqual("Player-" + id1, player1.NickName);
        Assert.AreEqual("Player-" + id2, player2.NickName);
        Assert.AreEqual("Player-" + id3, player3.NickName);
        Assert.IsNull(store.Get(id1));
    }

    [TestMethod]
    public void Test_NamedLruMemoryStore_ManualCreate()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNamedLruMemoryStoreManualCreate<Player, Guid>(maxCachedItemsCount: 3);

        var serviceProvider = services.BuildServiceProvider();
        var namedProvider =
            serviceProvider.GetRequiredService<NamedLruMemoryStoreManualCreatedProvider<Player, Guid>>();
        var store = namedProvider.GetStore("test-store");

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        var id4 = Guid.NewGuid();

        // Act
        var player1 = store.GetOrAdd(id1, id => new Player(id, $"Player-{id}"));
        var player2 = store.GetOrAdd(id2, id => new Player(id, $"Player-{id}"));
        _ = store.GetOrAdd(id3, id => new Player(id, $"Player-{id}"));
        store.GetOrAdd(id4, id => new Player(id, $"Player-{id}")); // Evicts id1 due to LRU

        // Assert
        Assert.AreEqual("Player-" + id1, player1.NickName);
        Assert.AreEqual("Player-" + id2, player2.NickName);
        Assert.IsNull(store.Get(id1));
    }
}