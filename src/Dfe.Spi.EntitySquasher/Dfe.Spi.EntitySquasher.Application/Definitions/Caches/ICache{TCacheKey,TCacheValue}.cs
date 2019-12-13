namespace Dfe.Spi.EntitySquasher.Application.Definitions.Caches
{
    /// <summary>
    /// Describes the operations of a cache.
    /// </summary>
    /// <typeparam name="TCacheKey">
    /// The type of key used in the underlying storage.
    /// </typeparam>
    /// <typeparam name="TCacheValue">
    /// The type of item to store in the cache.
    /// </typeparam>
    public interface ICache<TCacheKey, TCacheValue>
        where TCacheValue : class
    {
        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="cacheItem">
        /// An instance of type <typeparamref name="TCacheValue" />.
        /// </param>
        void AddCacheItem(TCacheKey key, TCacheValue cacheItem);

        /// <summary>
        /// Gets an item from the cache, unless not found, in which case null.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// An instance of type <typeparamref name="TCacheValue" />, unless not
        /// found, then null.
        /// </returns>
        TCacheValue GetCacheItem(TCacheKey key);
    }
}