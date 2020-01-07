namespace Dfe.Spi.EntitySquasher.Application.Managers
{
    using System;
    using System.Collections.Generic;
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
        private readonly ILoggerWrapper loggerWrapper;

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
            this.loggerWrapper = loggerWrapper;
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
            this.loggerWrapper.Debug(
                $"Pulling back " +
                $"{nameof(AlgorithmConfigurationDeclarationFile)} for " +
                $"algorithm \"{algorithm}\" from the " +
                $"{nameof(IAlgorithmConfigurationDeclarationFileManager)}...");

            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm)
                    .ConfigureAwait(false);

            // 2) Pull back the base URL of the right
            //    adapter using the name and;
            if (algorithmConfigurationDeclarationFile != null)
            {
                this.loggerWrapper.Info(
                    $"Pulled back " +
                    $"{algorithmConfigurationDeclarationFile}. " +
                    $"Searching for {nameof(EntityAdapter)} with " +
                    $"{nameof(name)} \"{name}\"...");

                EntityAdapter entityAdapter =
                    algorithmConfigurationDeclarationFile.EntityAdapters
                        .SingleOrDefault(x => x.Name == name);

                // 2) Create the EntityAdapterClient via the
                //    factory and return it.
                if (entityAdapter != null)
                {
                    this.loggerWrapper.Info(
                        $"Found matching {nameof(EntityAdapter)}: " +
                        $"{entityAdapter}.");

                    Uri baseUrl = entityAdapter.BaseUrl;

                    this.loggerWrapper.Debug($"{nameof(baseUrl)} = {baseUrl}");

                    Dictionary<string, string> headers = entityAdapter.Headers;

                    this.loggerWrapper.Debug(
                        $"{nameof(headers)} = {headers.Count} item(s)");

                    toReturn = this.entityAdapterClientFactory.Create(
                        name,
                        baseUrl,
                        headers);

                    this.loggerWrapper.Info(
                        $"Created {nameof(IEntityAdapterClient)} with " +
                        $"{nameof(baseUrl)} = {baseUrl}.");
                }
                else
                {
                    this.loggerWrapper.Warning(
                        $"Found {algorithmConfigurationDeclarationFile}, " +
                        $"but could not find {nameof(EntityAdapter)} with " +
                        $"{nameof(name)} = \"{name}\"!");
                }
            }
            else
            {
                this.loggerWrapper.Warning(
                    $"The " +
                    $"{nameof(IAlgorithmConfigurationDeclarationFileManager)} " +
                    $"did not return a " +
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} for " +
                    $"{nameof(algorithm)} \"{algorithm}\".");
            }

            return toReturn;
        }
    }
}