using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.EntitySquasher.Domain.Configuration;
using Dfe.Spi.EntitySquasher.Domain.Profiles;

namespace Dfe.Spi.EntitySquasher.Application.Profiles
{
    public class CachingProfileRepository : IProfileRepository
    {
        private static readonly Dictionary<string, CacheItem> Cache = new Dictionary<string, CacheItem>();
        
        private readonly IProfileRepository _innerRepository;
        private readonly EntitySquasherConfiguration _configuration;

        public CachingProfileRepository(
            IProfileRepository innerRepository,
            EntitySquasherConfiguration configuration)
        {
            _innerRepository = innerRepository;
            _configuration = configuration;
        }
        
        public async Task<Profile> GetProfileAsync(string name, CancellationToken cancellationToken)
        {
            var cacheKey = name.ToUpper();
            if (Cache.ContainsKey(cacheKey))
            {
                var cacheItem = Cache[cacheKey];
                if (DateTime.Now < cacheItem.Expiry)
                {
                    return cacheItem.Value;
                }

                Cache.Remove(cacheKey);
            }

            var profile = await _innerRepository.GetProfileAsync(name, cancellationToken);
            if (_configuration.Profile.CacheDurationSeconds.HasValue)
            {
                Cache.Add(cacheKey, new CacheItem
                {
                    Value = profile,
                    Expiry = DateTime.Now.AddSeconds(_configuration.Profile.CacheDurationSeconds.Value)
                });
            }

            return profile;
        }
        
        private class CacheItem
        {
            public DateTime Expiry { get; set; }
            public Profile Value { get; set; }
        }
    }
}