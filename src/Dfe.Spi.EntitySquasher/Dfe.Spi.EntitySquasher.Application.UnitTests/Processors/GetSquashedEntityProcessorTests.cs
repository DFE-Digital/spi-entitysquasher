namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.UnitTesting;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Application.Processors;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class GetSquashedEntityProcessorTests
    {
        private Mock<IEntityAdapterInvoker> mockEntityAdapterInvoker;
        private Mock<IGetSquashedEntityProcessorSettingsProvider> mockGetSquashedEntityProcessorSettingsProvider;
        private Assembly assembly;
        private GetSquashedEntityProcessor getSquashedEntityProcessor;
        private LoggerWrapper loggerWrapper;

        [SetUp]
        public void Arrange()
        {
            Type type = typeof(GetSquashedEntityProcessorTests);
            this.assembly = type.Assembly;

            this.mockEntityAdapterInvoker =
                new Mock<IEntityAdapterInvoker>();
            this.mockGetSquashedEntityProcessorSettingsProvider =
                new Mock<IGetSquashedEntityProcessorSettingsProvider>();
            Mock<IResultSquasher> mockResultSquasher =
                new Mock<IResultSquasher>();

            IEntityAdapterInvoker entityAdapterInvoker =
                mockEntityAdapterInvoker.Object;
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider =
                this.mockGetSquashedEntityProcessorSettingsProvider.Object;
            IResultSquasher resultSquasher = mockResultSquasher.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.getSquashedEntityProcessor = new GetSquashedEntityProcessor(
                entityAdapterInvoker,
                getSquashedEntityProcessorSettingsProvider,
                this.loggerWrapper,
                resultSquasher);
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

        [Test]
        public async Task UnnamedTest()
        {
            // Arrange
            string defaultAlgorithm = "default-example";

            this.mockGetSquashedEntityProcessorSettingsProvider
                .Setup(x => x.DefaultAlgorithm)
                .Returns(defaultAlgorithm);

            string getSquashedEntityRequestStr =
                this.assembly.GetSample("get-squashed-entity-request-1.json");

            GetSquashedEntityRequest getSquashedEntityRequest =
                JsonConvert.DeserializeObject<GetSquashedEntityRequest>(
                    getSquashedEntityRequestStr);

            string actualGetSquashedEntityResponseStr =
                this.assembly.GetSample(
                    "invoke-entity-adapters-result-1.json");

            InvokeEntityAdaptersResult invokeEntityAdaptersResult = 
                JsonConvert.DeserializeObject<InvokeEntityAdaptersResult>(
                    actualGetSquashedEntityResponseStr);

            this.mockEntityAdapterInvoker
                .Setup(x => x.InvokeEntityAdapters(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<EntityReference>()))
                .ReturnsAsync(invokeEntityAdaptersResult);

            GetSquashedEntityResponse getSquashedEntityResponse = null;

            // Act
            getSquashedEntityResponse =
                await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                    getSquashedEntityRequest);

            // Assert

        }
    }
}