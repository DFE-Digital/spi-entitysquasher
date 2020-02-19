namespace Dfe.Spi.EntitySquasher.FunctionApp.UnitTests.Functions
{
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.Common.UnitTesting;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.EntitySquasher.FunctionApp.Functions;
    using Dfe.Spi.Models.Entities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [TestFixture]
    public class GetSquashedEntityTests
    {
        private Assembly assembly;
        private GetSquashedEntity getSquashedEntity;
        private LoggerWrapper loggerWrapper;

        private Mock<IGetSquashedEntityProcessor> mockGetSquashedEntityProcessor;
        private Mock<IHttpErrorBodyResultProvider> mockHttpErrorBodyResultProvider;

        [SetUp]
        public void Arrange()
        {
            Type type = typeof(GetSquashedEntityTests);
            this.assembly = type.Assembly;

            this.mockGetSquashedEntityProcessor =
                new Mock<IGetSquashedEntityProcessor>();
            this.mockHttpErrorBodyResultProvider =
                new Mock<IHttpErrorBodyResultProvider>();
            Mock<IHttpSpiExecutionContextManager> mockHttpSpiExecutionContextManager =
                new Mock<IHttpSpiExecutionContextManager>();

            IGetSquashedEntityProcessor getSquashedEntityProcessor =
                mockGetSquashedEntityProcessor.Object;
            IHttpErrorBodyResultProvider httpErrorBodyResultProvider =
                mockHttpErrorBodyResultProvider.Object;
            IHttpSpiExecutionContextManager httpSpiExecutionContextManager =
                mockHttpSpiExecutionContextManager.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.getSquashedEntity = new GetSquashedEntity(
                getSquashedEntityProcessor,
                httpErrorBodyResultProvider,
                httpSpiExecutionContextManager,
                loggerWrapper);
        }

        [Test]
        public void RunAsync_PostWithoutHttpRequest_ThrowsNullArgumentException()
        {
            // Arrange
            HttpRequest httpRequest = null;
            CancellationToken cancellationToken = CancellationToken.None;

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.getSquashedEntity.RunAsync(
                        httpRequest,
                        cancellationToken);
                };

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(asyncTestDelegate);
        }

        [Test]
        public async Task RunAsync_PostWithoutBody_ReturnsBadRequestStatusCode()
        {
            HttpRequest httpRequest = this.CreateHttpRequest();
            CancellationToken cancellationToken = CancellationToken.None;

            await this.ReturnsBadRequestStatusCode(
                httpRequest,
                cancellationToken);
        }

        [Test]
        public async Task RunAsync_PostMalformedBody_ReturnsBadRequestStatusCode()
        {
            string requestBodyStr = "some rubbish goes here";

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);
            CancellationToken cancellationToken = CancellationToken.None;

            await this.ReturnsBadRequestStatusCode(
                httpRequest,
                cancellationToken);
        }

        [Test]
        public async Task RunAsync_PostWellFormedPayloadNotConformingToSchema_ReturnsBadRequestStatusCode()
        {
            string requestBodyStr = this.assembly.GetSample(
                "empty-object.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);
            CancellationToken cancellationToken;

            await this.ReturnsBadRequestStatusCode(
                httpRequest,
                cancellationToken);
        }

        [Test]
        public async Task RunAsync_PostWellFormedPayloadWithUnsupportedAlgorithm_ReturnsNotFoundStatusCode()
        {
            // Arrange
            this.mockGetSquashedEntityProcessor
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>(), It.IsAny<CancellationToken>()))
                .Throws<FileNotFoundException>();

            IActionResult actionResult = null;
            HttpErrorBodyResult httpErrorBodyResult = new HttpErrorBodyResult(
                HttpStatusCode.NotFound,
                null,
                null);

            string requestBodyStr = this.assembly.GetSample(
                "get-squashed-entity-request-1.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);
            CancellationToken cancellationToken = CancellationToken.None;

            this.mockHttpErrorBodyResultProvider
                .Setup(x => x.GetHttpErrorBodyResult(It.IsAny<HttpStatusCode>(), It.IsAny<int>(), It.IsAny<string[]>()))
                .Returns(httpErrorBodyResult);

            int? expectedStatusCode = (int)httpErrorBodyResult.StatusCode;
            int? actualStatusCode;

            // Act
            actionResult = await this.getSquashedEntity.RunAsync(
                httpRequest,
                cancellationToken);

            // Assert
            Assert.IsInstanceOf<HttpErrorBodyResult>(actionResult);

            httpErrorBodyResult = (HttpErrorBodyResult)actionResult;
            actualStatusCode = httpErrorBodyResult.StatusCode;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);

            // Log output...
            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public async Task RunAsync_PostWellFormedPayloadWithSupportedAlgorithmButInvalidAcdf_ReturnsInternalServerError()
        {
            // Arrange
            string message =
                "This is an exception message that will appear in the error " +
                "reported back to the client.";

            this.mockGetSquashedEntityProcessor
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    throw new InvalidAlgorithmConfigurationDeclarationFileException(
                        message);
                });

            IActionResult actionResult = null;
            HttpErrorBodyResult httpErrorBodyResult = new HttpErrorBodyResult(
                HttpStatusCode.InternalServerError,
                null,
                $"An error happened! {message}");

            string requestBodyStr = this.assembly.GetSample(
                "get-squashed-entity-request-1.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);
            CancellationToken cancellationToken = CancellationToken.None;

            this.mockHttpErrorBodyResultProvider
                .Setup(x => x.GetHttpErrorBodyResult(It.IsAny<HttpStatusCode>(), It.IsAny<int>(), It.IsAny<string[]>()))
                .Returns(httpErrorBodyResult);

            int? expectedStatusCode = (int)httpErrorBodyResult.StatusCode;
            int? actualStatusCode;

            HttpErrorBody httpErrorBody = null;
            string errorBodyMessage = null;

            // Act
            actionResult = await this.getSquashedEntity.RunAsync(
                httpRequest,
                CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<HttpErrorBodyResult>(actionResult);

            httpErrorBodyResult = (HttpErrorBodyResult)actionResult;
            actualStatusCode = httpErrorBodyResult.StatusCode;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);

            httpErrorBody =
                httpErrorBodyResult.Value as HttpErrorBody;

            errorBodyMessage = httpErrorBody.Message;

            Assert.IsTrue(errorBodyMessage.Contains(message));

            // Log output...
            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public async Task RunAsync_PostWellFormedPayloadWithSupportedAlgorithmOneAdapterFails_ReturnsWithLearningProviderBodyAndPartialSuccess()
        {
            GetSquashedEntityResponse getSquashedEntityResponse =
                new GetSquashedEntityResponse()
                {
                    SquashedEntityResults = new SquashedEntityResult[]
                    {
                        new SquashedEntityResult()
                        {
                            EntityAdapterErrorDetails = new EntityAdapterErrorDetail[]
                            {
                                new EntityAdapterErrorDetail()
                                {
                                    // An error happened...
                                }
                            }
                        },
                        new SquashedEntityResult()
                        {
                            SquashedEntity = new LearningProvider()
                            {
                                // However, there was at least one entity here.
                            }
                        }
                    }
                };

            await this.ReturnsBodyAndCorrectStatusCode(
                HttpStatusCode.PartialContent,
                getSquashedEntityResponse,
                CancellationToken.None);
        }

        [Test]
        public async Task RunAsync_PostWellFormedPayloadWithSupportedAlgorithmNoAdaptersFail_ReturnsWithLearningProviderBodyAndOk()
        {
            GetSquashedEntityResponse getSquashedEntityResponse =
                new GetSquashedEntityResponse()
                {
                    SquashedEntityResults = new SquashedEntityResult[]
                    {
                        new SquashedEntityResult()
                        {
                            EntityAdapterErrorDetails = new EntityAdapterErrorDetail[]
                            {
                                // Needs an empty array here.
                            }
                        },
                    }
                };

         await this.ReturnsBodyAndCorrectStatusCode(
                null, // It appears that null is the same as 200 to the runtime.
                getSquashedEntityResponse,
                CancellationToken.None);
        }

        [Test]
        public async Task RunAsync_PostWellFormedPayloadWithSupportedAlgorithmAllAdaptersFail_ReturnsWithFailedDependency()
        {
            // Arrange
            IActionResult actionResult = null;

            GetSquashedEntityResponse getSquashedEntityResponse =
                new GetSquashedEntityResponse()
                {
                    SquashedEntityResults = new SquashedEntityResult[]
                    {
                        new SquashedEntityResult()
                        {
                            EntityAdapterErrorDetails = new EntityAdapterErrorDetail[]
                            {
                                new EntityAdapterErrorDetail()
                                {
                                    // Don't need anythin' in here.
                                    // It just needs to exist.
                                },
                            },
                        },
                    },
                };

            this.mockGetSquashedEntityProcessor
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getSquashedEntityResponse));

            string requestBodyStr = this.assembly.GetSample(
                "get-squashed-entity-request-1.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            HttpErrorBodyResult httpErrorBodyResult = new HttpErrorBodyResult(
               HttpStatusCode.FailedDependency,
               null,
               null);

            int? expectedStatusCode = (int)httpErrorBodyResult.StatusCode;
            int? actualStatusCode = null;

            this.mockHttpErrorBodyResultProvider
                .Setup(x => x.GetHttpErrorBodyResult(It.IsAny<HttpStatusCode>(), It.IsAny<int>(), It.IsAny<string[]>()))
                .Returns(httpErrorBodyResult);

            // Act
            actionResult = await this.getSquashedEntity.RunAsync(
                httpRequest,
                CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<HttpErrorBodyResult>(actionResult);

            httpErrorBodyResult = (HttpErrorBodyResult)actionResult;
            actualStatusCode = httpErrorBodyResult.StatusCode;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);

            // Log output...
            string logOutput = this.loggerWrapper.ReturnLog();
        }

        private async Task ReturnsBodyAndCorrectStatusCode(
            HttpStatusCode? expectedStatusCode,
            GetSquashedEntityResponse expectedGetSquashedEntityResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            IActionResult actionResult = null;
            JsonResult jsonResult = null;

            GetSquashedEntityResponse actualGetSquashedEntityResponse = null;

            this.mockGetSquashedEntityProcessor
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(expectedGetSquashedEntityResponse));

            string requestBodyStr = this.assembly.GetSample(
                "get-squashed-entity-request-1.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            HttpStatusCode? actualStatusCode = null;

            // Act
            actionResult = await this.getSquashedEntity.RunAsync(
                httpRequest,
                cancellationToken);

            // Assert
            Assert.IsInstanceOf<JsonResult>(actionResult);

            jsonResult = (JsonResult)actionResult;
            actualGetSquashedEntityResponse = (GetSquashedEntityResponse)jsonResult.Value;

            Assert.AreEqual(
                expectedGetSquashedEntityResponse,
                actualGetSquashedEntityResponse);

            actualStatusCode = (HttpStatusCode?)jsonResult.StatusCode;
            Assert.AreEqual(
                expectedStatusCode,
                actualStatusCode);

            // Log output...
            string logOutput = this.loggerWrapper.ReturnLog();
        }

        private HttpRequest CreateHttpRequest(string bodyStr = null)
        {
            HttpRequest toReturn = new DefaultHttpRequest(
                new DefaultHttpContext());

            if (!string.IsNullOrEmpty(bodyStr))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(bodyStr);

                MemoryStream memoryStream = new MemoryStream(buffer);

                toReturn.Body = memoryStream;
            }

            return toReturn;
        }

        private async Task ReturnsBadRequestStatusCode(
            HttpRequest httpRequest,
            CancellationToken cancellationToken)
        {
            // Arrange
            IActionResult actionResult = null;
            HttpErrorBodyResult httpErrorBodyResult = new HttpErrorBodyResult(
                HttpStatusCode.BadRequest,
                null,
                null);

            int? expectedStatusCode = (int)httpErrorBodyResult.StatusCode;
            int? actualStatusCode;

            this.mockHttpErrorBodyResultProvider
                .Setup(x => x.GetHttpErrorBodyResult(It.IsAny<HttpStatusCode>(), It.IsAny<int>(), It.IsAny<string[]>()))
                .Returns(httpErrorBodyResult);

            // Act
            actionResult = await this.getSquashedEntity.RunAsync(
                httpRequest,
                cancellationToken);

            // Assert
            Assert.IsInstanceOf<HttpErrorBodyResult>(actionResult);

            httpErrorBodyResult = (HttpErrorBodyResult)actionResult;
            actualStatusCode = httpErrorBodyResult.StatusCode;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);

            // Log output...
            string logOutput = this.loggerWrapper.ReturnLog();
        }
    }
}