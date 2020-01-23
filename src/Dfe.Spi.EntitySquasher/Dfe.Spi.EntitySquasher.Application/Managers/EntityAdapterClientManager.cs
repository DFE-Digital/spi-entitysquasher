namespace Dfe.Spi.EntitySquasher.Application.Managers
{
    using Dfe.Spi.Common.Caching.Definitions.Caches;
    using Dfe.Spi.Common.Caching.Managers;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientManager" />.
    /// </summary>
    public class EntityAdapterClientManager
        : MemoryCacheManager<EntityAdapterClientKey, IEntityAdapterClient>, IEntityAdapterClientManager
    {
        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClientManager" /> class.
        /// </summary>
        /// <param name="memoryCacheProvider">
        /// An instance of type
        /// <see cref="IMemoryCacheProvider{EntityAdapterClientKey, IEntityAdapterClient}" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <param name="initialiseCacheItemAsync">
        /// An instance of
        /// <see cref="MemoryCacheManager{EntityAdapterClientKey, IEntityAdapterClient}.InitialiseCacheItemAsync" />.
        /// </param>
        public EntityAdapterClientManager(
            IMemoryCacheProvider<EntityAdapterClientKey, IEntityAdapterClient> memoryCacheProvider,
            ILoggerWrapper loggerWrapper,
            InitialiseCacheItemAsync initialiseCacheItemAsync)
            : base(
                  memoryCacheProvider,
                  loggerWrapper,
                  initialiseCacheItemAsync)
        {
            // Nothing - simply bubbles down.
        }
    }
}