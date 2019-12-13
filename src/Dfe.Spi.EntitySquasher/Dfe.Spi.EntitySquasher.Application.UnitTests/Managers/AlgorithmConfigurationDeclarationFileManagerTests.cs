﻿namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Managers
{
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Managers;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Moq;
    using NUnit.Framework;
    using System.Threading.Tasks;

    [TestFixture]
    public class AlgorithmConfigurationDeclarationFileManagerTests
    {
        private Mock<IAlgorithmConfigurationDeclarationFileCache> mockAlgorithmConfigurationDeclarationFileCache;
        private Mock<IAlgorithmConfigurationDeclarationFileStorageAdapter> mockAlgorithmConfigurationDeclarationFileStorageAdapter;

        private AlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager;
        private LoggerWrapper loggerWrapper;

        [SetUp]
        public void Arrange()
        {
            this.mockAlgorithmConfigurationDeclarationFileCache =
                new Mock<IAlgorithmConfigurationDeclarationFileCache>();
            this.mockAlgorithmConfigurationDeclarationFileStorageAdapter =
                new Mock<IAlgorithmConfigurationDeclarationFileStorageAdapter>();

            IAlgorithmConfigurationDeclarationFileCache algorithmConfigurationDeclarationFileCache =
                mockAlgorithmConfigurationDeclarationFileCache.Object;
            IAlgorithmConfigurationDeclarationFileStorageAdapter algorithmConfigurationDeclarationFileStorageAdapter =
                mockAlgorithmConfigurationDeclarationFileStorageAdapter.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.algorithmConfigurationDeclarationFileManager =
                new AlgorithmConfigurationDeclarationFileManager(
                    algorithmConfigurationDeclarationFileCache,
                    algorithmConfigurationDeclarationFileStorageAdapter,
                    loggerWrapper);
        }

        [Test]
        public async Task GetAsync_AcdfDoesNotExistForAlgorithm_ReturnsNull()
        {
            // Arrange
            string algorithm = "algorithm-that-doesnt-exist";
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile = null;

            // Act
            algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm);

            // Assert
            Assert.IsNull(algorithmConfigurationDeclarationFile);

            string logOutput = this.loggerWrapper.ReturnLog();
       }

        [Test]
        public async Task GetAsync_AcdfExistsInCacheForAlgorithm_ReturnsAcdf()
        {
            // Arrange
            AlgorithmConfigurationDeclarationFile expectedAlgorithmConfigurationDeclarationFile =
                new AlgorithmConfigurationDeclarationFile()
                {
                    // Doesn't need any configuration.. just needs to be
                    // returned...
                };
            AlgorithmConfigurationDeclarationFile actualAlgorithmConfigurationDeclarationFile = null;

            this.mockAlgorithmConfigurationDeclarationFileCache
                .Setup(x => x.GetCacheItem(It.IsAny<string>()))
                .Returns(expectedAlgorithmConfigurationDeclarationFile);

            string algorithm = "algorithm-that-does-exist";

            // Act
            actualAlgorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm);

            // Assert
            Assert.AreEqual(
                expectedAlgorithmConfigurationDeclarationFile,
                actualAlgorithmConfigurationDeclarationFile);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public async Task GetAsync_AcdfExistsInStorageForAlgorithm_ReturnsAcdf()
        {
            // Arrange
            AlgorithmConfigurationDeclarationFile expectedAlgorithmConfigurationDeclarationFile =
                new AlgorithmConfigurationDeclarationFile()
                {
                    // Doesn't need any configuration.. just needs to be
                    // returned...
                };
            AlgorithmConfigurationDeclarationFile actualAlgorithmConfigurationDeclarationFile = null;

            this.mockAlgorithmConfigurationDeclarationFileStorageAdapter
                .Setup(x => x.GetAlgorithmConfigurationDeclarationFileAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(expectedAlgorithmConfigurationDeclarationFile));

            string algorithm = "algorithm-that-does-exist";

            // Act
            actualAlgorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm);

            // Assert
            Assert.AreEqual(
                expectedAlgorithmConfigurationDeclarationFile,
                actualAlgorithmConfigurationDeclarationFile);

            string logOutput = this.loggerWrapper.ReturnLog();
        }
    }
}