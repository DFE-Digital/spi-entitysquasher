namespace Dfe.Spi.EntitySquasher.Application.Factories
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Caching;
    using Dfe.Spi.Common.Caching.Definitions.Managers;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmConfigurationDeclarationFileCacheManagerFactory" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileManagerFactory
        : IAlgorithmConfigurationDeclarationFileCacheManagerFactory
    {
        private readonly IAlgorithmConfigurationDeclarationFileCache algorithmConfigurationDeclarationFileCache;
        private readonly IAlgorithmConfigurationDeclarationFileStorageAdapter algorithmConfigurationDeclarationFileStorageAdapter;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmConfigurationDeclarationFileManagerFactory" />
        /// class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileCache">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileCacheManagerFactory" />.
        /// </param>
        /// <param name="algorithmConfigurationDeclarationFileStorageAdapter">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileStorageAdapter" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public AlgorithmConfigurationDeclarationFileManagerFactory(
            IAlgorithmConfigurationDeclarationFileCache algorithmConfigurationDeclarationFileCache,
            IAlgorithmConfigurationDeclarationFileStorageAdapter algorithmConfigurationDeclarationFileStorageAdapter,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileCache = algorithmConfigurationDeclarationFileCache;
            this.algorithmConfigurationDeclarationFileStorageAdapter = algorithmConfigurationDeclarationFileStorageAdapter;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public ICacheManager Create()
        {
            CacheManager toReturn =
                new CacheManager(
                    this.algorithmConfigurationDeclarationFileCache,
                    this.loggerWrapper,
                    this.InitialiseCacheItemAsync);

            return toReturn;
        }

        /// <inheritdoc />
        public async Task<object> InitialiseCacheItemAsync(
            string key,
            CancellationToken cancellationToken)
        {
            AlgorithmConfigurationDeclarationFile toReturn = null;

            this.loggerWrapper.Debug($"Fetching from storage...");

            toReturn = await this.algorithmConfigurationDeclarationFileStorageAdapter.GetAlgorithmConfigurationDeclarationFileAsync(
                key)
                .ConfigureAwait(false);

            if (toReturn != null)
            {
                this.loggerWrapper.Info(
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} " +
                    $"pulled from storage, for algorithm \"{key}\": " +
                    $"{toReturn}.");
            }
            else
            {
                this.loggerWrapper.Warning(
                    $"Could not find " +
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} in " +
                    $"storage for key \"{key}\"!");
            }

            return toReturn;
        }
    }
}