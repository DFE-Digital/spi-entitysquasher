namespace Dfe.Spi.EntitySquasher.Application.Managers
{
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;

    /// <summary>
    /// Implements <see cref="IManager{TCacheKey, TManagerItem}" />.
    /// </summary>
    /// <typeparam name="TCacheKey">
    /// The type of key used in the underlying storage.
    /// </typeparam>
    /// <typeparam name="TManagerItem">
    /// The type of item being managed.
    /// </typeparam>
    public abstract class Manager<TCacheKey, TManagerItem>
        : IManager<TCacheKey, TManagerItem>
        where TManagerItem : class
    {
        private readonly ICacheBase<TCacheKey, TManagerItem> cache;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="Manager{TCacheKey, TCacheValue}" /> class.
        /// </summary>
        /// <param name="cache">
        /// An instance of type <see cref="ICacheBase{TCacheKey, TManagerItem}" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public Manager(
            ICacheBase<TCacheKey, TManagerItem> cache,
            ILoggerWrapper loggerWrapper)
        {
            this.cache = cache;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<TManagerItem> GetAsync(TCacheKey key)
        {
            TManagerItem toReturn = null;

            string typeName = typeof(TManagerItem).Name;

            this.loggerWrapper.Debug(
                $"Checking the cache for an instance of {typeName} for " +
                $"algorithm \"{key}\"...");

            toReturn = this.cache.GetCacheItem(key);

            if (toReturn == null)
            {
                this.loggerWrapper.Debug(
                    $"No {typeName} found in cache with key \"{key}\". " +
                    $"Attempting to initialise a value for this key...");

                toReturn = await this.InitialiseCacheItem(key)
                    .ConfigureAwait(false);

                if (toReturn != null)
                {
                    this.loggerWrapper.Debug(
                        $"Storing {toReturn} in cache with key \"{key}\"...");

                    this.cache.AddCacheItem(key, toReturn);

                    this.loggerWrapper.Info(
                        $"{toReturn} stored in cache with key \"{key}\".");
                }
                else
                {
                    this.loggerWrapper.Warning(
                        $"The manager could not initialise a value for key " +
                        $"\"{key}\"!");
                }
            }
            else
            {
                this.loggerWrapper.Info(
                    $"{typeName} found in the cache for algorithm " +
                    $"\"{key}\": {toReturn}.");
            }

            return toReturn;
        }

        /// <summary>
        /// Initialises an item if it cannot be found in the cache.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// An instance of type
        /// <typeparamref name="TManagerItem" />.
        /// </returns>
        protected abstract Task<TManagerItem> InitialiseCacheItem(
            TCacheKey key);
    }
}