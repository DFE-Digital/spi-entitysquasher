﻿namespace Dfe.Spi.EntitySquasher.FunctionApp.UnitTests.Functions
{
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.UnitTesting;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.EntitySquasher.FunctionApp.Functions;
    using Dfe.Spi.Models;
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
    using System.Threading.Tasks;

    [TestFixture]
    public class GetSquashedEntityTests
    {
        private Assembly assembly;
        private GetSquashedEntity getSquashedEntity;
        private LoggerWrapper loggerWrapper;
        private Mock<IGetSquashedEntityProcessor> mockGetSquashedEntityProcessor;

        [SetUp]
        public void Arrange()
        {
            Type type = typeof(GetSquashedEntityTests);
            this.assembly = type.Assembly;

            this.mockGetSquashedEntityProcessor =
                new Mock<IGetSquashedEntityProcessor>();

            IGetSquashedEntityProcessor getSquashedEntityProcessor =
                mockGetSquashedEntityProcessor.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.getSquashedEntity = new GetSquashedEntity(
                getSquashedEntityProcessor,
                loggerWrapper);
        }

        [Test]
        public void Run_PostWithoutHttpRequest_ThrowsNullArgumentException()
        {
            // Arrange
            HttpRequest httpRequest = null;

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.getSquashedEntity.Run(httpRequest);
                };

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(asyncTestDelegate);
        }

        [Test]
        public async Task Run_PostWithoutBody_ReturnsBadRequestStatusCode()
        {
            HttpRequest httpRequest = this.CreateHttpRequest();

            await this.ReturnsBadRequestStatusCode(httpRequest);
        }

        [Test]
        public async Task Run_PostMalformedBody_ReturnsBadRequestStatusCode()
        {
            string requestBodyStr = "some rubbish goes here";

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            await this.ReturnsBadRequestStatusCode(httpRequest);
        }

        [Test]
        public async Task Run_PostWellFormedPayloadNotConformingToSchema_ReturnsBadRequestStatusCode()
        {
            string requestBodyStr = this.assembly.GetSample(
                "empty-object.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            await this.ReturnsBadRequestStatusCode(httpRequest);
        }

        [Test]
        public async Task Run_PostWellFormedPayloadWithUnsupportedAlgorithm_ReturnsNotFoundStatusCode()
        {
            // Arrange
            this.mockGetSquashedEntityProcessor
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>()))
                .Throws<FileNotFoundException>();

            IActionResult actionResult = null;
            HttpErrorBodyResult httpErrorBodyResult = null;

            string requestBodyStr = this.assembly.GetSample(
                "get-squashed-entity-request-1.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            int? expectedStatusCode = (int)HttpStatusCode.NotFound;
            int? actualStatusCode;

            // Act
            actionResult = await this.getSquashedEntity.Run(httpRequest);

            // Assert
            Assert.IsInstanceOf<HttpErrorBodyResult>(actionResult);

            httpErrorBodyResult = (HttpErrorBodyResult)actionResult;
            actualStatusCode = httpErrorBodyResult.StatusCode;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);

            // Log output...
            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public async Task Run_PostWellFormedPayloadWithSupportedAlgorithmOneAdapterFails_ReturnsWithLearningProviderBodyAndPartialSuccess()
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
                getSquashedEntityResponse);
        }

        [Test]
        public async Task Run_PostWellFormedPayloadWithSupportedAlgorithmNoAdaptersFail_ReturnsWithLearningProviderBodyAndOk()
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
                getSquashedEntityResponse);
        }

        [Test]
        public async Task Run_PostWellFormedPayloadWithSupportedAlgorithmAllAdaptersFail_ReturnsWithFailedDependency()
        {
            // Arrange
            IActionResult actionResult = null;
            JsonResult jsonResult = null;

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
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>()))
                .Returns(Task.FromResult(getSquashedEntityResponse));

            string requestBodyStr = this.assembly.GetSample(
                "get-squashed-entity-request-1.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            HttpErrorBodyResult httpErrorBodyResult = null;

            int? expectedStatusCode = (int)HttpStatusCode.FailedDependency;
            int? actualStatusCode = null;

            // Act
            actionResult = await this.getSquashedEntity.Run(httpRequest);

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
            GetSquashedEntityResponse expectedGetSquashedEntityResponse)
        {
            // Arrange
            IActionResult actionResult = null;
            JsonResult jsonResult = null;

            GetSquashedEntityResponse actualGetSquashedEntityResponse = null;

            this.mockGetSquashedEntityProcessor
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>()))
                .Returns(Task.FromResult(expectedGetSquashedEntityResponse));

            string requestBodyStr = this.assembly.GetSample(
                "get-squashed-entity-request-1.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            HttpStatusCode? actualStatusCode = null;

            // Act
            actionResult = await this.getSquashedEntity.Run(httpRequest);

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
            HttpRequest httpRequest)
        {
            // Arrange
            IActionResult actionResult = null;
            HttpErrorBodyResult httpErrorBodyResult = null;

            int? expectedStatusCode = (int)HttpStatusCode.BadRequest;
            int? actualStatusCode;

            // Act
            actionResult = await this.getSquashedEntity.Run(httpRequest);

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