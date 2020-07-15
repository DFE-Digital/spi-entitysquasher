using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.EntitySquasher.Application.Profiles;
using Dfe.Spi.EntitySquasher.Domain.Configuration;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Moq;
using NUnit.Framework;

namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Profiles.CachingProfileRepositoryTests
{
    public class WhenGettingProfiles
    {
        private Mock<IProfileRepository> _innerRepositoryMock;
        private EntitySquasherConfiguration _configuration;
        private CachingProfileRepository _cachingProfileRepository;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _innerRepositoryMock = new Mock<IProfileRepository>();

            _configuration = new EntitySquasherConfiguration
            {
                Profile = new ProfileConfiguration
                {
                    CacheDurationSeconds = 1,
                },
            };

            _cachingProfileRepository = new CachingProfileRepository(
                _innerRepositoryMock.Object,
                _configuration);

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldReturnValueFromInnerRepositoryIfFirstCall()
        {
            var profile = new Profile {Name = "Profile1"};
            _innerRepositoryMock.Setup(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            var actual = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);

            Assert.AreSame(profile, actual);
            _innerRepositoryMock.Verify(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldReturnValueFromCacheOnSubsequentCallsBeforeCacheItemExpires()
        {
            var profile = new Profile {Name = "Profile1"};
            var callCount = 0;
            _innerRepositoryMock.Setup(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string name, CancellationToken cancellationToken) =>
                {
                    callCount++;
                    return callCount == 1 ? profile : new Profile {Name = $"ProfileForCall{callCount}"};
                });

            var actual1 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);
            var actual2 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);
            var actual3 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);

            Assert.AreSame(profile, actual1);
            Assert.AreSame(profile, actual2);
            Assert.AreSame(profile, actual3);
            _innerRepositoryMock.Verify(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldReturnFreshValueFromRepositoryOnceCacheItemExpires()
        {
            var profile1 = new Profile {Name = "Profile1"};
            var profile2 = new Profile {Name = "Profile2"};
            var callCount = 0;
            _innerRepositoryMock.Setup(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string name, CancellationToken cancellationToken) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        return profile1;
                    }

                    if (callCount == 2)
                    {
                        return profile2;
                    }

                    return new Profile {Name = $"ProfileForCall{callCount}"};
                });

            var actual1 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);
            var actual2 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);
            await Task.Delay(1500);
            var actual3 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);

            Assert.AreSame(profile1, actual1);
            Assert.AreSame(profile1, actual2);
            Assert.AreSame(profile2, actual3);
            _innerRepositoryMock.Verify(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task ThenItShouldReturnFreshValueFromRepositoryEveryTimeIfCacheDurationNotConfigured()
        {
            _configuration.Profile.CacheDurationSeconds = null;
            var profile1 = new Profile {Name = "Profile1"};
            var profile2 = new Profile {Name = "Profile2"};
            var profile3 = new Profile {Name = "Profile3"};
            var callCount = 0;
            _innerRepositoryMock.Setup(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string name, CancellationToken cancellationToken) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        return profile1;
                    }

                    if (callCount == 2)
                    {
                        return profile2;
                    }

                    if (callCount == 3)
                    {
                        return profile3;
                    }

                    return new Profile {Name = $"ProfileForCall{callCount}"};
                });

            var actual1 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);
            var actual2 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);
            await Task.Delay(1000);
            var actual3 = await _cachingProfileRepository.GetProfileAsync("test", _cancellationToken);

            Assert.AreSame(profile1, actual1);
            Assert.AreSame(profile2, actual2);
            Assert.AreSame(profile3, actual3);
            _innerRepositoryMock.Verify(r => r.GetProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }
    }
}