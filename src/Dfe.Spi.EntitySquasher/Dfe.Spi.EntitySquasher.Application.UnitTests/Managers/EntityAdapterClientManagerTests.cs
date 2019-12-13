namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Managers
{
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class EntityAdapterClientManagerTests
    {
        private Mock<IAlgorithmConfigurationDeclarationFileManager> mockAlgorithmConfigurationDeclarationFileManager;
        private Mock<IEntityAdapterClientFactory> mockEntityAdapterClientFactory;
        private EntityAdapterClientManager entityAdapterClientManager;
        private LoggerWrapper loggerWrapper;

        [SetUp]
        public void Arrange()
        {
            this.mockAlgorithmConfigurationDeclarationFileManager =
                new Mock<IAlgorithmConfigurationDeclarationFileManager>();
            Mock<IEntityAdapterClientCache> mockEntityAdapterClientCache =
                new Mock<IEntityAdapterClientCache>();
            this.mockEntityAdapterClientFactory =
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
        public void GetAsync_ProvideNullKey_ThrowsArgumentNullException()
        {
            // Arrange
            EntityAdapterClientKey entityAdapterClientKey = null;

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.entityAdapterClientManager.GetAsync(
                        entityAdapterClientKey);
                };

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(asyncTestDelegate);
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

        [Test]
        public async Task GetAsync_AlgorithmDoesExistButAdapterDoesNot_ReturnsNull()
        {
            // Arrange
            EntityAdapterClientKey entityAdapterClientKey =
                new EntityAdapterClientKey()
                {
                    Algorithm = "algorithm-does-exist",
                    Name = "name-that-does-not-exist",
                };

            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                new AlgorithmConfigurationDeclarationFile()
                {
                    EntityAdapters = new EntityAdapter[]
                    {
                        // Empty array...
                    }
                };

            this.mockAlgorithmConfigurationDeclarationFileManager
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(algorithmConfigurationDeclarationFile));

            IEntityAdapterClient entityAdapterClient = null;

            // Act
            entityAdapterClient =
                await this.entityAdapterClientManager.GetAsync(
                    entityAdapterClientKey);

            // Assert
            Assert.IsNull(entityAdapterClient);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public async Task GetAsync_AlgorithmAndAdapterExists_ReturnsAdapterClient()
        {
            // Arrange
            const string name = "name-that-does-exist";

            EntityAdapterClientKey entityAdapterClientKey =
                new EntityAdapterClientKey()
                {
                    Algorithm = "algorithm-does-exist",
                    Name = name,
                };

            Uri baseUrl = new Uri(
                "https://somecorp.local/api/adapter",
                UriKind.Absolute);
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                new AlgorithmConfigurationDeclarationFile()
                {
                    EntityAdapters = new EntityAdapter[]
                    {
                        new EntityAdapter()
                        {
                            Name = name,
                            BaseUrl = baseUrl,
                        },
                    },
                };

            Mock<IEntityAdapterClient> mockEntityAdapterClient =
                new Mock<IEntityAdapterClient>();
            IEntityAdapterClient expectedEntityAdapterClient =
                mockEntityAdapterClient.Object;

            this.mockAlgorithmConfigurationDeclarationFileManager
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(algorithmConfigurationDeclarationFile));
            this.mockEntityAdapterClientFactory
                .Setup(x => x.Create(It.Is<Uri>(y => y == baseUrl)))
                .Returns(expectedEntityAdapterClient);

            IEntityAdapterClient actualEntityAdapterClient = null;

            // Act
            actualEntityAdapterClient =
                await this.entityAdapterClientManager.GetAsync(
                    entityAdapterClientKey);

            // Assert
            Assert.AreEqual(
                expectedEntityAdapterClient,
                actualEntityAdapterClient);

            string logOutput = this.loggerWrapper.ReturnLog();
        }
    }
}