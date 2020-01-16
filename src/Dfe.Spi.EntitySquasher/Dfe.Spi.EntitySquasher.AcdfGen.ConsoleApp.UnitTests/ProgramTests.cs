namespace Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp.UnitTests
{
    using System;
    using System.IO;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;
    using Dfe.Spi.EntitySquasher.AcdfGen.Models;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ProgramTests
    {
        private LoggerWrapper loggerWrapper;
        private Mock<IGenerateAlgorithmConfigurationDeclarationFileProcessor> mockGenerateAlgorithmConfigurationDeclarationFileProcessor;

        private Program program;

        [SetUp]
        public void Arrange()
        {
            this.loggerWrapper = new LoggerWrapper();

            this.mockGenerateAlgorithmConfigurationDeclarationFileProcessor =
                new Mock<IGenerateAlgorithmConfigurationDeclarationFileProcessor>();

            IGenerateAlgorithmConfigurationDeclarationFileProcessor generateAlgorithmConfigurationDeclarationFileProcessor =
                mockGenerateAlgorithmConfigurationDeclarationFileProcessor.Object;

            program = new Program(
                this.loggerWrapper,
                generateAlgorithmConfigurationDeclarationFileProcessor);
        }

        [Test]
        public void Run_OptionsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Options options = null;

            TestDelegate testDelegate =
                () =>
                {
                    // Act
                    this.program.Run(options);
                };

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void Run_ProcessorThrowsException_ExceptionBubbledUp()
        {
            // Arrange
            Options options = new Options()
            {
                Filename = "a-filename.json",
                AdapterNames = new string[]
                {
                    "some-adapter",
                    "someother-adapter",
                },
            };

            this.mockGenerateAlgorithmConfigurationDeclarationFileProcessor
                .Setup(x => x.GenerateAlgorithmConfigurationDeclarationFile(It.IsAny<GenerateAlgorithmConfigurationDeclarationFileRequest>()))
                .Throws<FileNotFoundException>();

            TestDelegate testDelegate =
                () =>
                {
                    // Act
                    this.program.Run(options);
                };

            // Assert
            Assert.Throws<FileNotFoundException>(testDelegate);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public void Run_ProcessorExecutesWithSuccess_ZeroExitCodeReturned()
        {
            // Arrange
            Options options = new Options()
            {
                Filename = "a-filename.json",
                AdapterNames = new string[]
                {
                    "some-adapter",
                    "someother-adapter",
                },
            };

            GenerateAlgorithmConfigurationDeclarationFileResponse generateAlgorithmConfigurationDeclarationFileResponse =
                new GenerateAlgorithmConfigurationDeclarationFileResponse()
                {
                    // Nothing, for now.
                };

            this.mockGenerateAlgorithmConfigurationDeclarationFileProcessor
                .Setup(x => x.GenerateAlgorithmConfigurationDeclarationFile(It.IsAny<GenerateAlgorithmConfigurationDeclarationFileRequest>()))
                .Returns(generateAlgorithmConfigurationDeclarationFileResponse);

            int expectedExitCode = 0;
            int actualExitCode;

            // Act
            actualExitCode = this.program.Run(options);

            // Assert
            Assert.AreEqual(expectedExitCode, actualExitCode);

            string logOutput = this.loggerWrapper.ReturnLog();
        }
    }
}