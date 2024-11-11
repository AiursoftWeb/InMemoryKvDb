using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aiursoft.InMemoryKvDb.AutoCreate;

namespace Aiursoft.InMemoryKvDb.Tests
{
    [TestClass]
    public class LruMemoryStoreTests
    {
        [TestMethod]
        public void Test_LruMemoryStore_AutoCreate_BasicOperations()
        {
            // Arrange
            var store = new LruMemoryStore<Player, Guid>(id => new Player(id, $"Player-{id}"), 2);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();

            // Act
            var player1 = store.GetOrAdd(id1);
            var player2 = store.GetOrAdd(id2);
            var player3 = store.GetOrAdd(id3); // Should evict id1

            // Assert
            Assert.AreEqual("Player-" + id1, player1.NickName);
            Assert.AreEqual("Player-" + id2, player2.NickName);
            Assert.AreEqual("Player-" + id3, player3.NickName);
            Assert.IsNull(store.Get(id1));
        }

        [TestMethod]
        public void Test_NamedLruMemoryStore_AutoCreate()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedLruMemoryStore<Player, Guid>(
                id => new Player(id, $"Player-{id}"),
                maxCachedItemsCount: 3);

            var serviceProvider = services.BuildServiceProvider();
            var namedProvider = serviceProvider.GetRequiredService<NamedLruMemoryStoreProvider<Player, Guid>>();
            var store = namedProvider.GetStore("test-store");

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var id4 = Guid.NewGuid();

            // Act
            var player1 = store.GetOrAdd(id1);
            var player2 = store.GetOrAdd(id2);
            _ = store.GetOrAdd(id3);
            store.GetOrAdd(id4); // Evicts id1 due to LRU

            // Assert
            Assert.AreEqual("Player-" + id1, player1.NickName);
            Assert.AreEqual("Player-" + id2, player2.NickName);
            Assert.IsNull(store.Get(id1));
        }
    }
}