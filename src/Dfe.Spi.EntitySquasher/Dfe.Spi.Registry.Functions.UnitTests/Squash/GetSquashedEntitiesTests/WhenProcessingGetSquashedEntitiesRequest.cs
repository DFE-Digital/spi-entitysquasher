using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Dfe.Spi.Common.Http.Server;
using Dfe.Spi.Common.Http.Server.Definitions;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.Models;
using Dfe.Spi.Common.UnitTesting;
using Dfe.Spi.EntitySquasher.Application.Squash;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Dfe.Spi.EntitySquasher.Functions;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Registry.Functions.Squash;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Dfe.Spi.Registry.Functions.UnitTests.Squash.GetSquashedEntitiesTests
{
    public class WhenProcessingGetSquashedEntitiesRequest
    {
        private Fixture _fixture;
        private Mock<ISquashManager> _squashManagerMock;
        private Mock<IHttpSpiExecutionContextManager> _httpSpiExecutionContextManagerMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private GetSquashedEntities _function;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize<SquashRequest>(b =>
                b.With(x => x.EntityName, nameof(LearningProvider)));
            
            _squashManagerMock = new Mock<ISquashManager>();
            _squashManagerMock.Setup(m => m.SquashAsync(It.IsAny<SquashRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SquashResponse
                {
                    SquashedEntityResults = new[]
                    {
                        new SquashedEntityResult
                        {
                            SquashedEntity = new LearningProvider(),
                        },
                    },
                });

            _httpSpiExecutionContextManagerMock = new Mock<IHttpSpiExecutionContextManager>();

            _loggerMock = new Mock<ILoggerWrapper>();

            _function = new GetSquashedEntities(
                _squashManagerMock.Object,
                _httpSpiExecutionContextManagerMock.Object,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldReturnBadRequestIfBodyIsNotJson()
        {
            var actual = await _function.RunAsync(GetRequest("not-json"), _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<HttpErrorBodyResult>(actual);

            var actualHttpErrorBodyResult = (HttpErrorBodyResult) actual;
            Assert.AreEqual(400, actualHttpErrorBodyResult.StatusCode);
            Assert.IsInstanceOf<HttpErrorBody>(actualHttpErrorBodyResult.Value);
            Assert.AreEqual("SPI-ESQ-1", ((HttpErrorBody) actualHttpErrorBodyResult.Value).ErrorIdentifier);
        }

        [Test]
        public async Task ThenItShouldReturnBadRequestIfBodyIsJsonButDoesNotConformToSchema()
        {
            var actual = await _function.RunAsync(GetRequest("{}"), _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<HttpSchemaValidationErrorBodyResult>(actual);

            var actualHttpErrorBodyResult = (HttpErrorBodyResult) actual;
            Assert.AreEqual(400, actualHttpErrorBodyResult.StatusCode);
            Assert.IsInstanceOf<HttpErrorBody>(actualHttpErrorBodyResult.Value);
            Assert.AreEqual("SPI-ESQ-2", ((HttpErrorBody) actualHttpErrorBodyResult.Value).ErrorIdentifier);
        }

        [Test]
        public async Task ThenItShouldCallSquashManagerAndReturnOkIfRequestFullyProcessed()
        {
            var request = _fixture.Create<SquashRequest>();

            var actual = await _function.RunAsync(GetRequest(request), _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<FormattedJsonResult>(actual);
            Assert.AreEqual(HttpStatusCode.OK, ((FormattedJsonResult) actual).StatusCode);
            _squashManagerMock.Verify(m => m.SquashAsync(
                It.Is<SquashRequest>(r => ObjectAssert.AreEqual(request, r)),
                _cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldCallSquashManagerAndReturnPartialContentIfRequestHasSomeResultsAndSomeErrorsProcessed()
        {
            var request = _fixture.Create<SquashRequest>();
            _squashManagerMock.Setup(m => m.SquashAsync(It.IsAny<SquashRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SquashResponse
                {
                    SquashedEntityResults = new[]
                    {
                        new SquashedEntityResult
                        {
                            SquashedEntity = new LearningProvider(),
                            EntityAdapterErrorDetails = new[]
                            {
                                new EntityAdapterErrorDetail(),
                            }
                        },
                    },
                });

            var actual = await _function.RunAsync(GetRequest(request), _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<FormattedJsonResult>(actual);
            Assert.AreEqual(HttpStatusCode.PartialContent, ((FormattedJsonResult) actual).StatusCode);
            _squashManagerMock.Verify(m => m.SquashAsync(
                It.Is<SquashRequest>(r => ObjectAssert.AreEqual(request, r)),
                _cancellationToken),
                Times.Once);
        }

        [Test,]
        public async Task ThenItShouldCallSquashManagerAndReturnFailedDependencyAllAdaptersFail()
        {
            var request = _fixture.Create<SquashRequest>();
            _squashManagerMock.Setup(m => m.SquashAsync(It.IsAny<SquashRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SquashResponse
                {
                    SquashedEntityResults = new[]
                    {
                        new SquashedEntityResult
                        {
                            EntityAdapterErrorDetails = new[]
                            {
                                new EntityAdapterErrorDetail(),
                            }
                        },
                    },
                });

            var actual = await _function.RunAsync(GetRequest(request), _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<HttpErrorBodyResult>(actual);
            Assert.AreEqual((int) HttpStatusCode.FailedDependency, ((HttpErrorBodyResult) actual).StatusCode);
            _squashManagerMock.Verify(m => m.SquashAsync(
                    It.Is<SquashRequest>(r => ObjectAssert.AreEqual(request, r)),
                    _cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldCallSquashManagerAndReturnBadRequestIfProfileNotFoundThrown()
        {
            var request = _fixture.Create<SquashRequest>();
            _squashManagerMock.Setup(m => m.SquashAsync(It.IsAny<SquashRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProfileNotFoundException("test"));
            
            var actual = await _function.RunAsync(GetRequest(request), _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<HttpErrorBodyResult>(actual);

            var actualHttpErrorBodyResult = (HttpErrorBodyResult) actual;
            Assert.AreEqual(400, actualHttpErrorBodyResult.StatusCode);
            Assert.IsInstanceOf<HttpErrorBody>(actualHttpErrorBodyResult.Value);
            Assert.AreEqual("SPI-ESQ-3", ((HttpErrorBody) actualHttpErrorBodyResult.Value).ErrorIdentifier);
        }

        [Test]
        public async Task ThenItShouldCallSquashManagerAndReturnBadRequestIfInvalidRequestThrown()
        {
            var request = _fixture.Create<SquashRequest>();
            _squashManagerMock.Setup(m => m.SquashAsync(It.IsAny<SquashRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidRequestException("test"));
            
            var actual = await _function.RunAsync(GetRequest(request), _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<HttpErrorBodyResult>(actual);

            var actualHttpErrorBodyResult = (HttpErrorBodyResult) actual;
            Assert.AreEqual(400, actualHttpErrorBodyResult.StatusCode);
            Assert.IsInstanceOf<HttpErrorBody>(actualHttpErrorBodyResult.Value);
            Assert.AreEqual("SPI-ESQ-7", ((HttpErrorBody) actualHttpErrorBodyResult.Value).ErrorIdentifier);
        }

        private HttpRequest GetRequest(SquashRequest request)
        {
            return GetRequest(JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            }));
        }

        private HttpRequest GetRequest(string body)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(body)),
            };
        }
    }
}