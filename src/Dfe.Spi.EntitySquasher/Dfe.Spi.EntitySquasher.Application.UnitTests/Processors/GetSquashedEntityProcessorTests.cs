namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.Common.UnitTesting;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Application.Processors;
    using Dfe.Spi.EntitySquasher.Domain;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.Models.Entities;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class GetSquashedEntityProcessorTests
    {
        private Mock<IEntityAdapterInvoker> mockEntityAdapterInvoker;
        private Mock<IGetSquashedEntityProcessorSettingsProvider> mockGetSquashedEntityProcessorSettingsProvider;
        private Mock<IResultSquasher> mockResultSquasher;
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
            this.mockResultSquasher =
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
            CancellationToken cancellationToken = CancellationToken.None;

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                        getSquashedEntityRequest,
                        cancellationToken);
                };

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(asyncTestDelegate);
        }

        [Test]
        public async Task GetSquashedEntityAsync_EntityAdapterInvokerThrowsAllAdaptersUnavailableException_OnlyErrorsReturned()
        {
            // Arrange
            string getSquashedEntityRequestStr =
                this.assembly.GetSample("get-squashed-entity-request-1.json");

            GetSquashedEntityRequest getSquashedEntityRequest =
                JsonConvert.DeserializeObject<GetSquashedEntityRequest>(
                    getSquashedEntityRequestStr);
            CancellationToken cancellationToken = CancellationToken.None;    

            HttpStatusCode httpStatusCode = HttpStatusCode.AlreadyReported;

            HttpErrorBody httpErrorBody = new HttpErrorBody()
            {
                ErrorIdentifier = "ABC",
                Message = "Some Error Message",
                StatusCode = httpStatusCode,
            };

            EntityAdapterErrorDetail expectedEntityAdapterErrorDetail =
                new EntityAdapterErrorDetail()
                {
                    AdapterName = "some-adapter",
                    HttpErrorBody = httpErrorBody,
                    HttpStatusCode = httpStatusCode,
                    RequestedEntityName = "LearningProvider",
                    RequestedFields = new string[]
                        {
                            "Name",
                        },
                    RequestedId = "9c9f835f-723d-4461-bd9d-e7b955c45623",
                };

            EntityAdapterException entityAdapterException =
                new EntityAdapterException(
                    expectedEntityAdapterErrorDetail,
                    httpStatusCode,
                    httpErrorBody);

            AllAdaptersUnavailableException allAdaptersUnavailableException =
                new AllAdaptersUnavailableException(
                    new EntityAdapterException[]
                    {
                        entityAdapterException,
                    });

            this.mockEntityAdapterInvoker
                .Setup(x => x.InvokeEntityAdaptersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<AggregatesRequest>(), It.IsAny<EntityReference>(), It.IsAny<CancellationToken>()))
                .Throws(allAdaptersUnavailableException);

            EntityBase entityBase = null;

            SquashedEntityResult squashedEntityResult = null;
            GetSquashedEntityResponse getSquashedEntityResponse = null;
            EntityAdapterErrorDetail actualEntityAdapterErrorDetail = null;

            // Act
            getSquashedEntityResponse =
                await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                    getSquashedEntityRequest,
                    cancellationToken);

            // Assert
            squashedEntityResult =
                getSquashedEntityResponse.SquashedEntityResults.Single();

            actualEntityAdapterErrorDetail =
                squashedEntityResult.EntityAdapterErrorDetails.Single();

            Assert.AreEqual(
                expectedEntityAdapterErrorDetail,
                actualEntityAdapterErrorDetail);

            entityBase = squashedEntityResult.SquashedEntity;

            Assert.IsNull(entityBase);
        }

        [Test]
        public async Task GetSquashedEntityAsync_UnderlyingServicesReturnOneModelAndOneError_SquashesCorrectlyAndReturnsError()
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
            CancellationToken cancellationToken = CancellationToken.None;

            string actualGetSquashedEntityResponseStr =
                this.assembly.GetSample(
                    "invoke-entity-adapters-result-1.json");

            InvokeEntityAdaptersResult invokeEntityAdaptersResult =
                JsonConvert.DeserializeObject<InvokeEntityAdaptersResult>(
                    actualGetSquashedEntityResponseStr);

            GetEntityAsyncResult getEntityAsyncResult =
                invokeEntityAdaptersResult.GetEntityAsyncResults.First();

            HttpStatusCode httpStatusCode = HttpStatusCode.AlreadyReported;

            HttpErrorBody httpErrorBody = new HttpErrorBody()
            {
                ErrorIdentifier = "ABC",
                Message = "Some Error Message",
                StatusCode = httpStatusCode,
            };

            EntityAdapterErrorDetail expectedEntityAdapterErrorDetail =
                new EntityAdapterErrorDetail()
                {
                    AdapterName = "some-adapter",
                    HttpErrorBody = httpErrorBody,
                    HttpStatusCode = httpStatusCode,
                    RequestedEntityName = "LearningProvider",
                    RequestedFields = new string[]
                        {
                            "Name",
                        },
                    RequestedId = "9c9f835f-723d-4461-bd9d-e7b955c45623",
                };

            getEntityAsyncResult.EntityAdapterException =
                new EntityAdapterException(
                    expectedEntityAdapterErrorDetail,
                    httpStatusCode,
                    httpErrorBody);

            this.mockEntityAdapterInvoker
                .Setup(x => x.InvokeEntityAdaptersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<AggregatesRequest>(), It.IsAny<EntityReference>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(invokeEntityAdaptersResult);

            Spi.Models.Entities.LearningProvider learningProvider =
                new Spi.Models.Entities.LearningProvider()
                {
                    Name = "squashed thing",
                };
            Action<string, string, IEnumerable<GetEntityAsyncResult>, AggregatesRequest, CancellationToken> callback =
                (v, w, x, y, z) =>
                {
                    // Enumerate the collection, to provide coverage.
                    x.ToArray();
                };

            this.mockResultSquasher
                .Setup(x => x.SquashAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<GetEntityAsyncResult>>(), It.IsAny<AggregatesRequest>(), It.IsAny<CancellationToken>()))
                .Callback(callback)
                .ReturnsAsync(learningProvider);

            GetSquashedEntityResponse getSquashedEntityResponse = null;

            SquashedEntityResult squashedEntityResult = null;
            EntityAdapterErrorDetail actualEntityAdapterErrorDetail = null;

            EntityBase entityBase = null;

            // Act
            getSquashedEntityResponse =
                await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                    getSquashedEntityRequest,
                    cancellationToken);

            // Assert
            squashedEntityResult = getSquashedEntityResponse
                .SquashedEntityResults
                .First();

            actualEntityAdapterErrorDetail = squashedEntityResult
                .EntityAdapterErrorDetails
                .First();

            Assert.AreEqual(
                expectedEntityAdapterErrorDetail,
                actualEntityAdapterErrorDetail);

            squashedEntityResult = getSquashedEntityResponse
                .SquashedEntityResults
                .Last();

            entityBase = squashedEntityResult.SquashedEntity;

            Assert.IsNotNull(entityBase);
        }
    }
}