namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Processors
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.UnitTesting;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Processors;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class GetSquashedEntityProcessorTests
    {
        private Assembly assembly;
        private GetSquashedEntityProcessor getSquashedEntityProcessor;
        private LoggerWrapper loggerWrapper;

        [SetUp]
        public void Arrange()
        {
            Type type = typeof(GetSquashedEntityProcessorTests);
            this.assembly = type.Assembly;

            Mock<IEntityAdapterInvoker> mockEntityAdapterInvoker =
                new Mock<IEntityAdapterInvoker>();
            Mock<IGetSquashedEntityProcessorSettingsProvider> mockGetSquashedEntityProcessorSettingsProvider =
                new Mock<IGetSquashedEntityProcessorSettingsProvider>();
            Mock<IResultSquasher> mockResultSquasher =
                new Mock<IResultSquasher>();

            IEntityAdapterInvoker entityAdapterInvoker =
                mockEntityAdapterInvoker.Object;
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider =
                mockGetSquashedEntityProcessorSettingsProvider.Object;
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

        public async Task UnnamedTest()
        {
            // Arrange
            string getSquashedEntityRequestStr =
                this.assembly.GetSample("get-squashed-entity-request-1.json");

            GetSquashedEntityRequest getSquashedEntityRequest =
                JsonConvert.DeserializeObject<GetSquashedEntityRequest>(getSquashedEntityRequestStr);

            GetSquashedEntityResponse actualGetSquashedEntityResponse = null;

            // Act
            actualGetSquashedEntityResponse =
                await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                    getSquashedEntityRequest);

            // Assert

        }
    }
}