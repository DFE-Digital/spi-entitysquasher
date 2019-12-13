namespace Dfe.Spi.EntitySquasher.Application.Caches
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Implements <see cref="ICache{TCacheItem}" />.
    /// </summary>
    /// <typeparam name="TCacheItem">
    /// The type of item to store in the cache.
    /// </typeparam>
    public abstract class Cache<TCacheItem> : ICache<TCacheItem>
        where TCacheItem : class
    {
        private readonly Dictionary<string, TCacheItem> cache;

        /// <summary>
        /// Initialises a new instance of the <see cref="Cache{TCacheItem}" />
        /// class.
        /// </summary>
        public Cache()
        {
            this.cache = new Dictionary<string, TCacheItem>();
        }

        /// <inheritdoc />
        public void AddCacheItem(string key, TCacheItem cacheItem)
        {
            // We should never need to overwrite what's in the cache.
            if (!this.cache.ContainsKey(key))
            {
                // ... so just check that the key doesn't exist.
                this.cache.Add(key, cacheItem);
            }
        }

        /// <inheritdoc />
        public TCacheItem GetCacheItem(string key)
        {
            TCacheItem toReturn = null;

            if (this.cache.ContainsKey(key))
            {
                toReturn = this.cache[key];
            }

            return toReturn;
        }
    }
}