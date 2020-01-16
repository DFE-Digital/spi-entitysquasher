namespace Dfe.Spi.EntitySquasher.AcdfGen.Application.UnitTests.Processors
{
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LoggerWrapper = Common.UnitTesting.Infrastructure.LoggerWrapper;

    [TestFixture]
    public class GenerateAlgorithmConfigurationDeclarationFileProcessorTests
    {
        private Mock<IGeneratedAlgorithmConfigurationDeclarationFileRepository> mockGeneratedAlgorithmConfigurationDeclarationFileRepository;
        private GenerateAlgorithmConfigurationDeclarationFileProcessor generateAlgorithmConfigurationDeclarationFileProcessor;
        private LoggerWrapper loggerWrapper;

        [SetUp]
        public void Arrange()
        {
            this.mockGeneratedAlgorithmConfigurationDeclarationFileRepository =
                new Mock<IGeneratedAlgorithmConfigurationDeclarationFileRepository>();
            IGeneratedAlgorithmConfigurationDeclarationFileRepository generatedAlgorithmConfigurationDeclarationFileRepository =
                mockGeneratedAlgorithmConfigurationDeclarationFileRepository.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.generateAlgorithmConfigurationDeclarationFileProcessor =
                new GenerateAlgorithmConfigurationDeclarationFileProcessor(
                    generatedAlgorithmConfigurationDeclarationFileRepository,
                    this.loggerWrapper);
        }

        [Test]
        public void GenerateAlgorithmConfigurationDeclarationFile_NullRequestInstance_ThrowsArgumentNullException()
        {
            // Arrange
            GenerateAlgorithmConfigurationDeclarationFileRequest generateAlgorithmConfigurationDeclarationFileRequest = null;

            TestDelegate testDelegate =
                () =>
                {
                    // Act
                    this.generateAlgorithmConfigurationDeclarationFileProcessor.GenerateAlgorithmConfigurationDeclarationFile(
                        generateAlgorithmConfigurationDeclarationFileRequest);
                };

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void GenerateAlgorithmConfigurationDeclarationFile_ExecutionCompletesSuccessfully_AcdfChecked()
        {
            // Arrange
            string[] adapterNames = new string[]
            {
                "adapter-one",
                "adapter-two",
            };
            string filename = "adcf.json";

            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile = null;
            IEnumerable<Entity> entities = null;
            IEnumerable<EntityAdapter> entityAdapters = null;
            Func<string, AlgorithmConfigurationDeclarationFile, string> saveCallback =
                (x, y) =>
                {
                    string filePath = $"C:\\{x}";

                    algorithmConfigurationDeclarationFile = y;

                    return filePath;
                };

            this.mockGeneratedAlgorithmConfigurationDeclarationFileRepository
                .Setup(x => x.Save(It.IsAny<string>(), It.IsAny<AlgorithmConfigurationDeclarationFile>()))
                .Returns(saveCallback);

            GenerateAlgorithmConfigurationDeclarationFileRequest generateAlgorithmConfigurationDeclarationFileRequest =
                new GenerateAlgorithmConfigurationDeclarationFileRequest()
                {
                    AdapterNames = adapterNames,
                    Filename = filename, 
                };

            GenerateAlgorithmConfigurationDeclarationFileResponse generateAlgorithmConfigurationDeclarationFileResponse = null;

            int expectedAdapterCount = adapterNames.Length;
            int actualAdapterCount;

            Entity modelsBaseEntity = null;

            // Act
            generateAlgorithmConfigurationDeclarationFileResponse =
                this.generateAlgorithmConfigurationDeclarationFileProcessor.GenerateAlgorithmConfigurationDeclarationFile(
                    generateAlgorithmConfigurationDeclarationFileRequest);

            // Assert
            // This should be sent to the persistance layer. So shouldn't
            // be null.
            Assert.IsNotNull(algorithmConfigurationDeclarationFile);

            entityAdapters =
                algorithmConfigurationDeclarationFile.EntityAdapters;

            Assert.IsNotNull(entityAdapters);

            // The number of generated adapters should match that of the number
            // of requested ones.
            actualAdapterCount = entityAdapters.Count();

            Assert.AreEqual(expectedAdapterCount, actualAdapterCount);

            entities = algorithmConfigurationDeclarationFile.Entities;

            Assert.IsNotNull(entities);

            // Given the number of entities will increase, just make sure that
            // a) It's over 1 and;
            // b) Doesn't include any abstract classes, such as ModelsBase.
            Assert.IsTrue(entities.Count() > 1);

            modelsBaseEntity = entities
                .SingleOrDefault(x => x.Name == nameof(Spi.Models.ModelsBase));

            Assert.IsNull(modelsBaseEntity);

            string logOutput = this.loggerWrapper.ReturnLog();
        }
    }
}