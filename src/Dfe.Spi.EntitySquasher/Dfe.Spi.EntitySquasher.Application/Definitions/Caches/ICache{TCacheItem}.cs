namespace Dfe.Spi.EntitySquasher.Application.Definitions.Caches
{
    /// <summary>
    /// Describes the operations of a cache.
    /// </summary>
    /// <typeparam name="TCacheItem">
    /// The item to store in the cache.
    /// </typeparam>
    public interface ICache<TCacheItem>
        where TCacheItem : class
    {
        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="cacheItem">
        /// An instance of type <typeparamref name="TCacheItem" />.
        /// </param>
        void AddCacheItem(string key, TCacheItem cacheItem);

        /// <summary>
        /// Gets an item from the cache, unless not found, in which case null.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// An instance of type <typeparamref name="TCacheItem" />, unless not
        /// found, then null.
        /// </returns>
        TCacheItem GetCacheItem(string key);
    }
}