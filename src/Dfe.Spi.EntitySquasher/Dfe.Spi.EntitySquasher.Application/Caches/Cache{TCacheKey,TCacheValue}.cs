namespace Dfe.Spi.EntitySquasher.Application.Caches
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Implements <see cref="ICache{TCacheKey,TCacheItem}" />.
    /// </summary>
    /// <typeparam name="TCacheKey">
    /// The type of key used in the underlying storage.
    /// </typeparam>
    /// <typeparam name="TCacheValue">
    /// The type of item to store in the cache.
    /// </typeparam>
    public abstract class Cache<TCacheKey, TCacheValue>
        : ICache<TCacheKey, TCacheValue>
        where TCacheValue : class
    {
        private readonly Dictionary<TCacheKey, TCacheValue> cache;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="Cache{TCacheKey, TCacheValue}" /> class.
        /// </summary>
        public Cache()
        {
            this.cache = new Dictionary<TCacheKey, TCacheValue>();
        }

        /// <inheritdoc />
        public void AddCacheItem(TCacheKey key, TCacheValue cacheItem)
        {
            // We should never need to overwrite what's in the cache.
            if (!this.cache.ContainsKey(key))
            {
                // ... so just check that the key doesn't exist.
                this.cache.Add(key, cacheItem);
            }
        }

        /// <inheritdoc />
        public TCacheValue GetCacheItem(TCacheKey key)
        {
            TCacheValue toReturn = null;

            if (this.cache.ContainsKey(key))
            {
                toReturn = this.cache[key];
            }

            return toReturn;
        }
    }
}