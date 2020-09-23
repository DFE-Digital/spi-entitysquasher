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
        private const string ManagementGroupEntityName = nameof(ManagementGroup);
        private const string LearningProviderRatesEntityName = nameof(LearningProviderRates);
        private const string ManagementGroupRatesEntityName = nameof(ManagementGroupRates);
        private const string CensusEntityName = nameof(Census);

        private readonly IProfileRepository _profileRepository;
        private readonly ITypedSquasher<LearningProvider> _learningProviderSquasher;
        private readonly ITypedSquasher<ManagementGroup> _managementGroupSquasher;
        private readonly ITypedSquasher<LearningProviderRates> _learningProviderRatesSquasher;
        private readonly ITypedSquasher<ManagementGroupRates> _managementGroupRatesSquasher;
        private readonly ITypedSquasher<Census> _censusSquasher;
        private readonly ILoggerWrapper _logger;

        public SquashManager(
            IProfileRepository profileRepository,
            ITypedSquasher<LearningProvider> learningProviderSquasher,
            ITypedSquasher<ManagementGroup> managementGroupSquasher,
            ITypedSquasher<LearningProviderRates> learningProviderRatesSquasher,
            ITypedSquasher<ManagementGroupRates> managementGroupRatesSquasher,
            ITypedSquasher<Census> censusSquasher,
            ILoggerWrapper logger)
        {
            _profileRepository = profileRepository;
            _learningProviderSquasher = learningProviderSquasher;
            _managementGroupSquasher = managementGroupSquasher;
            _learningProviderRatesSquasher = learningProviderRatesSquasher;
            _managementGroupRatesSquasher = managementGroupRatesSquasher;
            _censusSquasher = censusSquasher;
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
                    request.PointInTime,
                    profile,
                    cancellationToken);
            }
            
            if (request.EntityName.Equals(ManagementGroupEntityName, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Info("Squashing management groups");
                return await _managementGroupSquasher.SquashAsync(
                    request.EntityReferences,
                    request.AggregatesRequest,
                    request.Fields ?? new string[0],
                    request.Live,
                    request.PointInTime,
                    profile,
                    cancellationToken);
            }
            
            if (request.EntityName.Equals(LearningProviderRatesEntityName, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Info("Squashing learning provider rates");
                return await _learningProviderRatesSquasher.SquashAsync(
                    request.EntityReferences,
                    request.AggregatesRequest,
                    request.Fields ?? new string[0],
                    request.Live,
                    request.PointInTime,
                    profile,
                    cancellationToken);
            }
            
            if (request.EntityName.Equals(ManagementGroupRatesEntityName, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Info("Squashing management group rates");
                return await _managementGroupRatesSquasher.SquashAsync(
                    request.EntityReferences,
                    request.AggregatesRequest,
                    request.Fields ?? new string[0],
                    request.Live,
                    request.PointInTime,
                    profile,
                    cancellationToken);
            }
            
            if (request.EntityName.Equals(CensusEntityName, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Info("Squashing censuses");
                return await _censusSquasher.SquashAsync(
                    request.EntityReferences,
                    request.AggregatesRequest,
                    request.Fields ?? new string[0],
                    request.Live,
                    request.PointInTime,
                    profile,
                    cancellationToken);
            }

            throw new InvalidRequestException($"Unsupported entity type {request.EntityName}");
        }
    }
}