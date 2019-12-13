namespace Dfe.Spi.EntitySquasher.Application.Managers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientManager" />.
    /// </summary>
    public class EntityAdapterClientManager
        : Manager<EntityAdapterClientKey, IEntityAdapterClient>, IEntityAdapterClientManager
    {
        private readonly IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager;
        private readonly IEntityAdapterClientFactory entityAdapterClientFactory;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClientManager" /> class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileManager">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
        /// </param>
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
            IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager,
            IEntityAdapterClientCache entityAdapterClientCache,
            IEntityAdapterClientFactory entityAdapterClientFactory,
            ILoggerWrapper loggerWrapper)
            : base(
                  entityAdapterClientCache,
                  loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileManager = algorithmConfigurationDeclarationFileManager;
            this.entityAdapterClientFactory = entityAdapterClientFactory;
        }

        /// <inheritdoc />
        protected override async Task<IEntityAdapterClient> InitialiseCacheItem(
            EntityAdapterClientKey key)
        {
            IEntityAdapterClient toReturn = null;

            if (key == null)
            {
                throw new ArgumentNullException(
                    nameof(key));
            }

            string algorithm = key.Algorithm;
            string name = key.Name;

            // 1) Pull back the ACDF from the manager and;
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm)
                    .ConfigureAwait(false);

            // 2) Pull back the base URL of the right
            //    adapter using the name and;
            if (algorithmConfigurationDeclarationFile != null)
            {
                EntityAdapter entityAdapter =
                    algorithmConfigurationDeclarationFile.EntityAdapters
                        .SingleOrDefault(x => x.Name == name);

                // 2) Create the EntityAdapterClient via the
                //    factory and return it.
                if (entityAdapter != null)
                {
                    Uri baseUrl = entityAdapter.BaseUrl;

                    toReturn = this.entityAdapterClientFactory.Create(
                        baseUrl);
                }
            }

            return toReturn;
        }
    }
}