using Aiursoft.InMemoryKvDb.AutoCreate;
using Aiursoft.InMemoryKvDb.ManualCreate;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.InMemoryKvDb.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void Test_AddLruMemoryStore_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLruMemoryStore<Player, Guid>(
            id => new Player(id, "TestPlayer"),
            maxCachedItemsCount: 3);

        var serviceProvider = services.BuildServiceProvider();
        var store = serviceProvider.GetService<LruMemoryStore<Player, Guid>>();

        // Assert
        Assert.IsNotNull(store, "LruMemoryStore should be registered and resolved correctly.");
        var id = Guid.NewGuid();
        var player = store.GetOrAdd(id);
        Assert.AreEqual("TestPlayer", player.NickName, "Player nickname should match the provided 'onNotFound' logic.");
    }

    [TestMethod]
    public void Test_AddLruMemoryStoreManualCreate_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLruMemoryStoreManualCreate<Player, Guid>(maxCachedItemsCount: 3);

        var serviceProvider = services.BuildServiceProvider();
        var store = serviceProvider.GetService<LruMemoryStoreManualCreated<Player, Guid>>();

        // Assert
        Assert.IsNotNull(store, "LruMemoryStoreManualCreated should be registered and resolved correctly.");
        var id = Guid.NewGuid();
        var player = store.GetOrAdd(id, iid => new Player(iid, "TestPlayer"));
        Assert.AreEqual("TestPlayer", player.NickName, "Player nickname should match the manually provided logic.");
    }
}
