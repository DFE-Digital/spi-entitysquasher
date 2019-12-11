namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Processors
{
    using System;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Processors;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GetSquashedEntityProcessorTests
    {
        private GetSquashedEntityProcessor getSquashedEntityProcessor;
        private LoggerWrapper loggerWrapper;

        [SetUp]
        public void Arrange()
        {
            Mock<IAlgorithmConfigurationDeclarationFileManager> mockAlgorithmConfigurationDeclarationFileManager =
                new Mock<IAlgorithmConfigurationDeclarationFileManager>();
            Mock<IGetSquashedEntityProcessorSettingsProvider> mockGetSquashedEntityProcessorSettingsProvider =
                new Mock<IGetSquashedEntityProcessorSettingsProvider>();

            IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager =
                mockAlgorithmConfigurationDeclarationFileManager.Object;
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider =
                mockGetSquashedEntityProcessorSettingsProvider.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.getSquashedEntityProcessor = new GetSquashedEntityProcessor(
                algorithmConfigurationDeclarationFileManager,
                getSquashedEntityProcessorSettingsProvider,
                this.loggerWrapper);
        }

        [Test]
        public void GetSquashedEntityAsync_PostWithoutRequest_ThrowsArgumentNullException()
        {
            // Arrange
            GetSquashedEntityRequest getSquashedEntityRequest = null;

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                        getSquashedEntityRequest);
                };

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(asyncTestDelegate);
        }
    }
}