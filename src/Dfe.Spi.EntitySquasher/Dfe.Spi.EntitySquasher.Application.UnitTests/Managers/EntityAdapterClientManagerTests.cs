namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Managers
{
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
    using Moq;
    using NUnit.Framework;
    using System.Threading.Tasks;

    [TestFixture]
    public class EntityAdapterClientManagerTests
    {
        private EntityAdapterClientManager entityAdapterClientManager;
        private LoggerWrapper loggerWrapper;

        [SetUp]
        public void Arrange()
        {
            Mock<IAlgorithmConfigurationDeclarationFileManager> mockAlgorithmConfigurationDeclarationFileManager =
                new Mock<IAlgorithmConfigurationDeclarationFileManager>();
            Mock<IEntityAdapterClientCache> mockEntityAdapterClientCache =
                new Mock<IEntityAdapterClientCache>();
            Mock<IEntityAdapterClientFactory> mockEntityAdapterClientFactory =
                new Mock<IEntityAdapterClientFactory>();

            IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager =
                mockAlgorithmConfigurationDeclarationFileManager.Object;
            IEntityAdapterClientCache entityAdapterClientCache =
                mockEntityAdapterClientCache.Object;
            IEntityAdapterClientFactory entityAdapterClientFactory =
                mockEntityAdapterClientFactory.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.entityAdapterClientManager =
                new EntityAdapterClientManager(
                    algorithmConfigurationDeclarationFileManager,
                    entityAdapterClientCache,
                    entityAdapterClientFactory,
                    this.loggerWrapper);
        }

        [Test]
        public async Task GetAsync_AlgorithmNameDoesNotExist_ReturnsNull()
        {
            // Arrange
            EntityAdapterClientKey entityAdapterClientKey =
                new EntityAdapterClientKey()
                {
                    Algorithm = "algorithm-that-doesn't-exist",
                    Name = "name-that-does-not-exist",
                };

            IEntityAdapterClient entityAdapterClient = null;

            // Act
            entityAdapterClient =
                await this.entityAdapterClientManager.GetAsync(
                    entityAdapterClientKey);

            // Assert
            Assert.IsNull(entityAdapterClient);

            string logOutput = this.loggerWrapper.ReturnLog();
        }
    }
}