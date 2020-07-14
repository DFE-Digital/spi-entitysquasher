using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Dfe.Spi.Models.Entities;

namespace Dfe.Spi.EntitySquasher.Application.Squash
{
    public interface ISquashManager
    {
        Task<SquashResponse> SquashAsync(SquashRequest request, CancellationToken cancellationToken);
    }

    public class SquashManager : ISquashManager
    {
        private const string LearningProviderEntityName = nameof(LearningProvider);

        private readonly IProfileRepository _profileRepository;
        private readonly ITypedSquasher<LearningProvider> _learningProviderSquasher;
        private readonly ILoggerWrapper _logger;

        public SquashManager(
            IProfileRepository profileRepository,
            ITypedSquasher<LearningProvider> learningProviderSquasher,
            ILoggerWrapper logger)
        {
            _profileRepository = profileRepository;
            _learningProviderSquasher = learningProviderSquasher;
            _logger = logger;
        }

        public async Task<SquashResponse> SquashAsync(SquashRequest request, CancellationToken cancellationToken)
        {
            var profileName = request.Algorithm ?? "default";
            
            _logger.Info($"Loading profile {profileName}");
            var profile = await _profileRepository.GetProfileAsync(profileName, cancellationToken);
            if (profile == null)
            {
                throw new ProfileNotFoundException(profileName);
            }

            var result = await SquashAsync(request, profile, cancellationToken);
            return new SquashResponse
            {
                SquashedEntityResults = result,
            };
        }

        private async Task<SquashedEntityResult[]> SquashAsync(SquashRequest request, Profile profile, CancellationToken cancellationToken)
        {
            if (request.EntityName.Equals(LearningProviderEntityName, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Info("Squashing learning providers");
                return await _learningProviderSquasher.SquashAsync(
                    request.EntityReferences,
                    request.AggregatesRequest,
                    request.Fields ?? new string[0],
                    request.Live,
                    profile,
                    cancellationToken);
            }

            throw new InvalidRequestException($"Unsupported entity type {request.EntityName}");
        }
    }
}