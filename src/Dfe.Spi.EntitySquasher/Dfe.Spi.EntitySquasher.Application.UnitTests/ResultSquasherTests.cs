namespace Dfe.Spi.EntitySquasher.Application.UnitTests
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Caching.Definitions.Managers;
    using Dfe.Spi.Common.UnitTesting;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Dfe.Spi.Models;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ResultSquasherTests
    {
        private Mock<ICacheManager> mockCacheManager;

        private LoggerWrapper loggerWrapper;
        private ResultSquasher resultSquasher;

        [SetUp]
        public void Arrange()
        {
            this.loggerWrapper = new LoggerWrapper();

            this.mockCacheManager = new Mock<ICacheManager>();

            ICacheManager cacheManager = mockCacheManager.Object;

            Mock<IAlgorithmConfigurationDeclarationFileCacheManagerFactory> mockAlgorithmConfigurationDeclarationFileCacheManagerFactory =
                new Mock<IAlgorithmConfigurationDeclarationFileCacheManagerFactory>();

            mockAlgorithmConfigurationDeclarationFileCacheManagerFactory
                .Setup(x => x.Create())
                .Returns(cacheManager);

            IAlgorithmConfigurationDeclarationFileCacheManagerFactory algorithmConfigurationDeclarationFileCacheManagerFactory =
                mockAlgorithmConfigurationDeclarationFileCacheManagerFactory.Object;

            this.resultSquasher = new ResultSquasher(
                algorithmConfigurationDeclarationFileCacheManagerFactory,
                this.loggerWrapper);
        }

        [Test]
        public void Ctor_PostWithoutEntityAdapterClientCacheManagerFactory_ThrowsArgumentNullException()
        {
            // Arrange
            IAlgorithmConfigurationDeclarationFileCacheManagerFactory algorithmConfigurationDeclarationFileCacheManagerFactory = null;

            TestDelegate testDelegate =
                () =>
                {
                    // Act
                    new ResultSquasher(
                        algorithmConfigurationDeclarationFileCacheManagerFactory,
                        this.loggerWrapper);
                };

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void SquashAsync_FeedWithUnknownEntityName_ThrowsInvalidAlgorithmConfigurationDeclarationFileException()
        {
            // Arrange
            string algorithm = "something-something";
            string entityName = "DoesntExist";
            GetEntityAsyncResult[] getEntityAsyncResults =
                new GetEntityAsyncResult[]
                {
                    // Empty - don't actually need anything for the purposes of this test.
                };
            CancellationToken cancellationToken = CancellationToken.None;

            this.ConfigureAlgorithmConfigurationDeclarationFileManager(
                "acdf-example.json");

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.resultSquasher.SquashAsync(
                        algorithm,
                        entityName,
                        getEntityAsyncResults,
                        cancellationToken);
                };

            // Assert
            Assert.ThrowsAsync<InvalidAlgorithmConfigurationDeclarationFileException>(
                asyncTestDelegate);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public void SquashAsync_FeedEntityAsyncResultsAndAcdfWithNoSources_ThrowsInvalidAlgorithmConfigurationDeclarationFileException()
        {
            // Arrange
            string algorithm = "something-something";
            string entityName = nameof(LearningProvider);
            GetEntityAsyncResult[] getEntityAsyncResults =
                new GetEntityAsyncResult[]
                {
                    // Empty - don't actually need anything for the purposes of this test.
                };
            CancellationToken cancellationToken = CancellationToken.None;

            this.ConfigureAlgorithmConfigurationDeclarationFileManager(
                "acdf-nosources.json");

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.resultSquasher.SquashAsync(
                        algorithm,
                        entityName,
                        getEntityAsyncResults,
                        cancellationToken);
                };

            // Assert
            Assert.ThrowsAsync<InvalidAlgorithmConfigurationDeclarationFileException>(
                asyncTestDelegate);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public async Task SquashAsync_FeedEntityAsyncResultsAndExampleAcdf_NameResultIsExpected()
        {
            // Arrange
            string algorithm = "some-algorithm";
            string entityName = nameof(LearningProvider);

            string expectedNameVariation = "SomeCorp";
            string actualNameVariation = null;

            string expectedLegalNameVariation = "SOMECORP LTD";
            string actualLegalNameVariation = null;

            string expectedPostcode = "LE1 1SU";
            string actualPostcode = null;

            long? expectedUkprn = 888;
            long? actualUkprn = null;

            long? expectedUrn = 98765;
            long? actualUrn = null;

            GetEntityAsyncResult[] toSquash = new GetEntityAsyncResult[]
            {
                new GetEntityAsyncResult()
                {
                     AdapterRecordReference = new AdapterRecordReference()
                     {
                         Id = "123",
                         Source = "someother-adapter"
                     },
                     ModelsBase = new LearningProvider()
                     {
                         Name = "  ",
                         Postcode = "LE1 1ST",
                         Ukprn = expectedUkprn,
                         Urn = 777,
                         LegalName = "SomeCorp Ltd"
                     },
                },
                new GetEntityAsyncResult()
                {
                    AdapterRecordReference = new AdapterRecordReference()
                    {
                        Id = "abc",
                        Source = "some-adapter",
                    },
                    ModelsBase = new LearningProvider()
                    {
                        LegalName = expectedLegalNameVariation,
                        Postcode = expectedPostcode,
                        Ukprn = 12345,
                        Urn = expectedUrn,
                        Name = expectedNameVariation,
                    },
                },
            };

            CancellationToken cancellationToken = CancellationToken.None;

            this.ConfigureAlgorithmConfigurationDeclarationFileManager(
                "acdf-example.json");

            Spi.Models.ModelsBase modelsBase = null;
            Spi.Models.LearningProvider learningProvider = null;

            // Act
            modelsBase = await this.resultSquasher.SquashAsync(
                algorithm,
                entityName,
                toSquash,
                cancellationToken);

            // Assert
            Assert.IsInstanceOf<LearningProvider>(modelsBase);

            learningProvider = modelsBase as LearningProvider;

            actualNameVariation = learningProvider.Name;
            Assert.AreEqual(expectedNameVariation, actualNameVariation);

            actualLegalNameVariation = learningProvider.LegalName;
            Assert.AreEqual(
                expectedLegalNameVariation,
                actualLegalNameVariation);

            actualPostcode = learningProvider.Postcode;
            Assert.AreEqual(expectedPostcode, actualPostcode);
            
            actualUkprn = learningProvider.Ukprn;
            Assert.AreEqual(expectedUkprn, actualUkprn);

            actualUrn = learningProvider.Urn;
            Assert.AreEqual(expectedUrn, actualUrn);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        private void ConfigureAlgorithmConfigurationDeclarationFileManager(
            string name)
        {
            Assembly assembly = typeof(ResultSquasherTests).Assembly;

            string algorithmConfigurationDeclarationFileStr =
                assembly.GetSample(name);

            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                JsonConvert.DeserializeObject<AlgorithmConfigurationDeclarationFile>(
                    algorithmConfigurationDeclarationFileStr);

            this.mockCacheManager
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(algorithmConfigurationDeclarationFile);
        }
    }
}