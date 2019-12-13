namespace Dfe.Spi.EntitySquasher.Application.Managers
{
    using System;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientManager" />.
    /// </summary>
    public class EntityAdapterClientManager
        : Manager<EntityAdapterClientKey, IEntityAdapterClient>, IEntityAdapterClientManager
    {
        private readonly IEntityAdapterClientFactory entityAdapterClientFactory;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClientManager" /> class.
        /// </summary>
        /// <param name="entityAdapterClientCache">
        /// An instance of type <see cref="IEntityAdapterClientCache" />.
        /// </param>
        /// <param name="entityAdapterClientFactory">
        /// An instance of type <see cref="IEntityAdapterClientFactory" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public EntityAdapterClientManager(
            IEntityAdapterClientCache entityAdapterClientCache,
            IEntityAdapterClientFactory entityAdapterClientFactory,
            ILoggerWrapper loggerWrapper)
            : base(
                  entityAdapterClientCache,
                  loggerWrapper)
        {
            this.entityAdapterClientFactory = entityAdapterClientFactory;
        }

        /// <inheritdoc />
        protected override Task<IEntityAdapterClient> InitialiseCacheItem(
            EntityAdapterClientKey key)
        {
            Task<IEntityAdapterClient> toReturn = null;

            if (key == null)
            {
                throw new ArgumentNullException(
                    nameof(key));
            }

            string algorithm = key.Algorithm;
            string name = key.Name;

            // TODO:
            // 1) Pull back the ACDF from the manager and;
            // 2) Pull back the base URL of the right
            //    adapter using the name and;
            // 2) Create the EntityAdapterClient via the
            //    factory and return it.
            return toReturn;
        }
    }
}