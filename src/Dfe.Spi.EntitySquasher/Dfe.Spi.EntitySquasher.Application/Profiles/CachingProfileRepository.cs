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
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

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
            
            // Quick initial check of cache; expect it to be cached more often than not
            if (_cache.ContainsKey(cacheKey))
            {
                var cacheItem = _cache[cacheKey];
                if (DateTime.Now < cacheItem.Expiry) // Check the cache is still valid
                {
                    return cacheItem.Value;
                }
            }

            Profile profile;
            try
            {
                // Get exclusive lock
                await Semaphore.WaitAsync(cancellationToken);
                
                // Re-check; another thread may have loaded while we were waiting
                if (_cache.ContainsKey(cacheKey))
                {
                    var cacheItem = _cache[cacheKey];
                    if (DateTime.Now < cacheItem.Expiry) // Check the cache is still valid
                    {
                        return cacheItem.Value;
                    }
                    
                    _cache.Remove(cacheKey);
                }

                // Definitely not there. Reload
                profile = await _innerRepository.GetProfileAsync(name, cancellationToken);
                if (_configuration.Profile.CacheDurationSeconds.HasValue)
                {
                    _cache.Add(cacheKey, new CacheItem
                    {
                        Value = profile,
                        Expiry = DateTime.Now.AddSeconds(_configuration.Profile.CacheDurationSeconds.Value)
                    });
                }
            }
            finally
            {
                Semaphore.Release();
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