namespace Dfe.Spi.EntitySquasher.Application.Managers
{
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileManager
        : Manager<string, AlgorithmConfigurationDeclarationFile>, IAlgorithmConfigurationDeclarationFileManager
    {
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
            : base(
                  algorithmConfigurationDeclarationFileCache,
                  loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileStorageAdapter = algorithmConfigurationDeclarationFileStorageAdapter;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        protected override async Task<AlgorithmConfigurationDeclarationFile> InitialiseCacheItem(
            string key)
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