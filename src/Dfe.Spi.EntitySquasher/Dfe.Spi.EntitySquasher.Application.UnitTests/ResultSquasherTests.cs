namespace Dfe.Spi.EntitySquasher.Application.UnitTests
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.UnitTesting;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
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
        private Mock<IAlgorithmConfigurationDeclarationFileManager> mockAlgorithmConfigurationDeclarationFileManager;

        private LoggerWrapper loggerWrapper;

        private ResultSquasher resultSquasher;

        [SetUp]
        public void Arrange()
        {
            this.loggerWrapper = new LoggerWrapper();

            this.mockAlgorithmConfigurationDeclarationFileManager =
                new Mock<IAlgorithmConfigurationDeclarationFileManager>();

            IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager =
                mockAlgorithmConfigurationDeclarationFileManager.Object;

            this.resultSquasher = new ResultSquasher(
                algorithmConfigurationDeclarationFileManager,
                this.loggerWrapper);
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

            this.ConfigureAlgorithmConfigurationDeclarationFileManager(
                "acdf-example.json");

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.resultSquasher.SquashAsync(
                        algorithm,
                        entityName,
                        getEntityAsyncResults);
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

            this.ConfigureAlgorithmConfigurationDeclarationFileManager(
                "acdf-nosources.json");

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.resultSquasher.SquashAsync(
                        algorithm,
                        entityName,
                        getEntityAsyncResults);
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

            string expectedNameVariation = "SomeCorp Limited";
            string actualNameVariation = null;

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
                         Name = null,
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
                        Name = expectedNameVariation,
                    },
                },
            };

            this.ConfigureAlgorithmConfigurationDeclarationFileManager(
                "acdf-example.json");

            Spi.Models.ModelsBase modelsBase = null;

            // Act
            modelsBase =
                await this.resultSquasher.SquashAsync(
                    algorithm,
                    entityName,
                    toSquash);

            // Assert
            actualNameVariation = modelsBase.Name;

            Assert.AreEqual(expectedNameVariation, actualNameVariation);

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

            this.mockAlgorithmConfigurationDeclarationFileManager
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(algorithmConfigurationDeclarationFile);
        }
    }
}