namespace Dfe.Spi.EntitySquasher.FunctionApp.UnitTests.Functions
{
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.FunctionApp.Functions;
    using Dfe.Spi.EntitySquasher.FunctionApp.Infrastructure;
    using Dfe.Spi.EntitySquasher.FunctionApp.UnitTests.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    [TestFixture]
    public class GetSquashedEntityTests
    {
        private GetSquashedEntity getSquashedEntity;
        private LoggerWrapper loggerWrapper;
        private Mock<IGetSquashedEntityProcessor> mockGetSquashedEntityProcessor;

        [SetUp]
        public void Arrange()
        {
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
        public async Task Run_PostWellFormedPayloadWithUnsupportedAlgorithm_ReturnsNotFoundStatusCode()
        {
            // Arrange
            this.mockGetSquashedEntityProcessor
                .Setup(x => x.GetSquashedEntityAsync(It.IsAny<GetSquashedEntityRequest>()))
                .Throws<FileNotFoundException>();

            IActionResult actionResult = null;
            StatusCodeResult statusCodeResult = null;

            string requestBodyStr = SamplesHelper.GetSample(
                "empty-object.json");

            HttpRequest httpRequest = this.CreateHttpRequest(requestBodyStr);

            int expectedStatusCode = StatusCodes.Status404NotFound;
            int actualStatusCode;

            // Act
            actionResult = await this.getSquashedEntity.Run(httpRequest);

            // Assert
            Assert.IsInstanceOf<StatusCodeResult>(actionResult);

            statusCodeResult = (StatusCodeResult)actionResult;
            actualStatusCode = statusCodeResult.StatusCode;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);

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

        private async Task ReturnsBadRequestStatusCode(HttpRequest httpRequest)
        {
            // Arrange
            IActionResult actionResult = null;
            StatusCodeResult statusCodeResult = null;

            int expectedStatusCode = StatusCodes.Status400BadRequest;
            int actualStatusCode;

            // Act
            actionResult = await this.getSquashedEntity.Run(httpRequest);

            // Assert
            Assert.IsInstanceOf<StatusCodeResult>(actionResult);

            statusCodeResult = (StatusCodeResult)actionResult;
            actualStatusCode = statusCodeResult.StatusCode;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);

            // Log output...
            string logOutput = this.loggerWrapper.ReturnLog();
        }
    }
}