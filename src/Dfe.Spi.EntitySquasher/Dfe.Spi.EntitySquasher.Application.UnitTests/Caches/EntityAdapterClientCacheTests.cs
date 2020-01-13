namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Caches
{
    using Dfe.Spi.EntitySquasher.Application.Caches;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EntityAdapterClientCacheTests
    {
        private EntityAdapterClientCache entityAdapterClientCache;

        [SetUp]
        public void Arrange()
        {
            this.entityAdapterClientCache =
                new EntityAdapterClientCache();
        }

        [Test]
        public void StoreIEntityAdapterClients_GetIEntityAdapterClientsBack()
        {
            // Arrange
            const string algorithm = "example-algorithm";
            const string name = "example-adapter";

            IEntityAdapterClient expectedItem1 = null,
                expectedItem2 = null,
                expectedItem3 = null,
                expectedItem4 = null;

            IEntityAdapterClient actualItem1 = null,
                actualItem2 = null,
                actualItem3 = null,
                actualItem4 = null;

            // Act
            expectedItem1 = AddMockIEntityAdapterClientToCache(
                null,
                null);
            expectedItem2 = AddMockIEntityAdapterClientToCache(
                null,
                name);
            expectedItem3 = AddMockIEntityAdapterClientToCache(
                algorithm,
                null);
            expectedItem4 = AddMockIEntityAdapterClientToCache(
                algorithm,
                name);

            actualItem1 = GetIEntityAdapterClientFromCache(
                null,
                null);
            actualItem2 = GetIEntityAdapterClientFromCache(
                null,
                name);
            actualItem3 = GetIEntityAdapterClientFromCache(
                algorithm,
                null);
            actualItem4 = GetIEntityAdapterClientFromCache(
                algorithm,
                name);

            // Assert
            Assert.AreEqual(expectedItem1, actualItem1);
            Assert.AreEqual(expectedItem2, actualItem2);
            Assert.AreEqual(expectedItem3, actualItem3);
            Assert.AreEqual(expectedItem4, actualItem4);
        }

        private IEntityAdapterClient GetIEntityAdapterClientFromCache(
            string algorithm,
            string name)
        {
            IEntityAdapterClient toReturn = null;

            EntityAdapterClientKey entityAdapterClientKey =
                new EntityAdapterClientKey()
                {
                    Algorithm = algorithm,
                    Name = name,
                };

            toReturn =
                this.entityAdapterClientCache.GetCacheItem(
                    entityAdapterClientKey);

            return toReturn;
        }

        private IEntityAdapterClient AddMockIEntityAdapterClientToCache(
            string algorithm,
            string name)
        {
            IEntityAdapterClient toReturn =
                CreateMockIEntityAdapterClient();

            EntityAdapterClientKey entityAdapterClientKey =
                new EntityAdapterClientKey()
                {
                    Algorithm = algorithm,
                    Name = name,
                };

            this.entityAdapterClientCache.AddCacheItem(
                entityAdapterClientKey,
                toReturn);

            return toReturn;
        }

        private static IEntityAdapterClient CreateMockIEntityAdapterClient()
        {
            IEntityAdapterClient toReturn = null;

            Mock<IEntityAdapterClient> mockEntityAdapterClient =
                new Mock<IEntityAdapterClient>();

            toReturn = mockEntityAdapterClient.Object;

            return toReturn;
        }
    }
}