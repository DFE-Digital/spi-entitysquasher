namespace Dfe.Spi.EntitySquasher.Application
{
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileManager
        : IAlgorithmConfigurationDeclarationFileManager
    {
        private readonly IAlgorithmConfigurationDeclarationFileCache algorithmConfigurationDeclarationFileCache;
        private readonly IAlgorithmConfigurationDeclarationFileStorageAdapter algorithmConfigurationDeclarationFileStorageAdapter;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmConfigurationDeclarationFileManager" /> class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileCache">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileCache" />.
        /// </param>
        /// <param name="algorithmConfigurationDeclarationFileStorageAdapter">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileStorageAdapter" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public AlgorithmConfigurationDeclarationFileManager(
            IAlgorithmConfigurationDeclarationFileCache algorithmConfigurationDeclarationFileCache,
            IAlgorithmConfigurationDeclarationFileStorageAdapter algorithmConfigurationDeclarationFileStorageAdapter,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileCache = algorithmConfigurationDeclarationFileCache;
            this.algorithmConfigurationDeclarationFileStorageAdapter = algorithmConfigurationDeclarationFileStorageAdapter;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<AlgorithmConfigurationDeclarationFile> GetAlgorithmConfigurationDeclarationFileAsync(
            string algorithm)
        {
            AlgorithmConfigurationDeclarationFile toReturn = null;

            this.loggerWrapper.Debug(
                $"Checking the cache for an instance of " +
                $"{nameof(AlgorithmConfigurationDeclarationFile)} for " +
                $"algorithm \"{algorithm}\"...");

            toReturn = this.algorithmConfigurationDeclarationFileCache.GetCacheItem(
                algorithm);

            if (toReturn == null)
            {
                this.loggerWrapper.Info(
                    $"No {nameof(AlgorithmConfigurationDeclarationFile)} " +
                    $"found in the cache for algorithm \"{algorithm}\". " +
                    $"Fetching from storage...");

                // 2) Otherwise, pull from the storage, store it in memory, then
                //    return.
                toReturn = await this.algorithmConfigurationDeclarationFileStorageAdapter.GetAlgorithmConfigurationDeclarationFileAsync(
                    algorithm)
                    .ConfigureAwait(false);

                this.loggerWrapper.Info(
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} " +
                    $"pulled from storage, for algorithm \"{algorithm}\": " +
                    $"{toReturn}. Storing in cache...");

                this.algorithmConfigurationDeclarationFileCache.AddCacheItem(
                    algorithm,
                    toReturn);

                this.loggerWrapper.Info(
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} " +
                    $"instance for \"{algorithm}\" stored in the cache.");
            }
            else
            {
                this.loggerWrapper.Info(
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} found " +
                    $"in the cache for algorithm \"{algorithm}\": " +
                    $"{toReturn}.");
            }

            return toReturn;
        }
    }
}