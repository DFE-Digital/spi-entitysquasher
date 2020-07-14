using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.EntitySquasher.Application.Squash;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Dfe.Spi.Models.Entities;
using Moq;
using NUnit.Framework;

namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Squash.SquashManagerTests
{
    public class WhenSquashing
    {
        private Fixture _fixture;
        private Mock<IProfileRepository> _profileRepositoryMock;
        private Mock<ITypedSquasher<LearningProvider>> _learningProviderSquasherMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private SquashManager _squashManager;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize<SquashRequest>(b =>
                b.With(x => x.EntityName, nameof(LearningProvider)));
            
            _profileRepositoryMock = new Mock<IProfileRepository>();
            _profileRepositoryMock.Setup(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Profile());
            
            _learningProviderSquasherMock = new Mock<ITypedSquasher<LearningProvider>>();
            
            _loggerMock = new Mock<ILoggerWrapper>();
            
            _squashManager = new SquashManager(
                _profileRepositoryMock.Object,
                _learningProviderSquasherMock.Object,
                _loggerMock.Object);
            
            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldGetProfileFromRepository()
        {
            var request = _fixture.Create<SquashRequest>();
            
            await _squashManager.SquashAsync(request, _cancellationToken);
            
            _profileRepositoryMock.Verify(r=>r.GetProfileAsync(request.Algorithm, _cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldThrowExceptionIfProfileNotFound()
        {
            var request = _fixture.Create<SquashRequest>();
            _profileRepositoryMock.Setup(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Profile)null);

            var actual = Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
                await _squashManager.SquashAsync(request, _cancellationToken));
            
            Assert.AreEqual(request.Algorithm, actual.ProfileName);
        }

        [Test]
        public async Task ThenItShouldThrowExceptionIfEntityTypeNotSupported()
        {
            var request = _fixture.Create<SquashRequest>();
            request.EntityName = "TestEntity";
            
            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _squashManager.SquashAsync(request, _cancellationToken));
            
            Assert.AreEqual("Unsupported entity type TestEntity", actual.Message);
        }

        [Test]
        public async Task ThenItShouldSquashLearningProvider()
        {
            var request = _fixture.Create<SquashRequest>();
            request.EntityName = nameof(LearningProvider);

            var profile = _fixture.Create<Profile>();
            _profileRepositoryMock.Setup(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            await _squashManager.SquashAsync(request, _cancellationToken);

            _learningProviderSquasherMock.Verify(s => s.SquashAsync(
                    request.EntityReferences,
                    request.AggregatesRequest,
                    request.Fields,
                    request.Live,
                    profile,
                    _cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldReturnSquasherResultForLearningProvider()
        {
            var request = _fixture.Create<SquashRequest>();
            request.EntityName = nameof(LearningProvider);

            var result = _fixture.Create<SquashedEntityResult[]>();
            _learningProviderSquasherMock.Setup(s => s.SquashAsync(
                    It.IsAny<EntityReference[]>(),
                    It.IsAny<AggregatesRequest>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Profile>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            
            var actual = await _squashManager.SquashAsync(request, _cancellationToken);
            
            Assert.IsNotNull(actual);
            Assert.AreSame(result, actual.SquashedEntityResults);
        }
    }
}