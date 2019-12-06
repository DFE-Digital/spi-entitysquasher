namespace Dfe.Spi.EntitySquasher.FunctionApp.UnitTests.Functions
{
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.FunctionApp.Functions;
    using Dfe.Spi.EntitySquasher.FunctionApp.Infrastructure;
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

        [SetUp]
        public void Arrange()
        {
            Mock<IGetSquashedEntityProcessor> mockGetSquashedEntityProcessor =
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
            HttpRequest httpRequest = new DefaultHttpRequest(
                new DefaultHttpContext());

            await this.ReturnsBadRequestStatusCode(httpRequest);
        }

        [Test]
        public async Task Run_PostMalformedBody_ReturnsBadRequestStatusCode()
        {
            HttpRequest httpRequest = new DefaultHttpRequest(
                new DefaultHttpContext());

            string requestBodyStr = "some rubbish goes here";

            byte[] buffer = Encoding.UTF8.GetBytes(requestBodyStr);

            MemoryStream memoryStream = new MemoryStream(buffer);

            httpRequest.Body = memoryStream;

            await this.ReturnsBadRequestStatusCode(httpRequest);
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