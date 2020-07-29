using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.EntitySquasher.Domain.Configuration;
using Dfe.Spi.EntitySquasher.Domain.Profiles;

namespace Dfe.Spi.EntitySquasher.Application.Profiles
{
    public class CachingProfileRepository : IProfileRepository
    {
        private readonly IProfileRepository _innerRepository;
        private readonly EntitySquasherConfiguration _configuration;
        private readonly Dictionary<string, CacheItem> _cache;

        public CachingProfileRepository(
            IProfileRepository innerRepository,
            EntitySquasherConfiguration configuration)
        {
            _innerRepository = innerRepository;
            _configuration = configuration;
            _cache = new Dictionary<string, CacheItem>();
        }
        
        public async Task<Profile> GetProfileAsync(string name, CancellationToken cancellationToken)
        {
            var cacheKey = name.ToUpper();
            if (_cache.ContainsKey(cacheKey))
            {
                var cacheItem = _cache[cacheKey];
                if (DateTime.Now < cacheItem.Expiry)
                {
                    return cacheItem.Value;
                }

                _cache.Remove(cacheKey);
            }

            var profile = await _innerRepository.GetProfileAsync(name, cancellationToken);
            if (_configuration.Profile.CacheDurationSeconds.HasValue)
            {
                _cache.Add(cacheKey, new CacheItem
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