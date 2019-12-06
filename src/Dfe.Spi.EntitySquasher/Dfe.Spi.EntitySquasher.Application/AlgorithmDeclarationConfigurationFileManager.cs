namespace Dfe.Spi.EntitySquasher.Application
{
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Adcf;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmDeclarationConfigurationFileManager" />.
    /// </summary>
    public class AlgorithmDeclarationConfigurationFileManager
        : IAlgorithmDeclarationConfigurationFileManager
    {
        private readonly IAlgorithmDeclarationConfigurationFileStorageAdapter algorithmDeclarationConfigurationFileStorageAdapter;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmDeclarationConfigurationFileManager" /> class.
        /// </summary>
        /// <param name="algorithmDeclarationConfigurationFileStorageAdapter">
        /// An instance of type
        /// <see cref="IAlgorithmDeclarationConfigurationFileStorageAdapter" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public AlgorithmDeclarationConfigurationFileManager(
            IAlgorithmDeclarationConfigurationFileStorageAdapter algorithmDeclarationConfigurationFileStorageAdapter,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmDeclarationConfigurationFileStorageAdapter = algorithmDeclarationConfigurationFileStorageAdapter;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<AlgorithmDeclarationConfigurationFile> GetAlgorithmDeclarationConfigurationFileAsync(
            string algoritm)
        {
            AlgorithmDeclarationConfigurationFile toReturn = null;

            // TODO:
            // 1) Check a singleton held in memory for the requested
            //    AlgorithmDeclarationConfigurationFile. If it exists, return
            //    it.
            // 2) Otherwise, pull from the storage, store it in memory, then
            //    return.
            toReturn = await this.algorithmDeclarationConfigurationFileStorageAdapter.GetAlgorithmDeclarationConfigurationFileAsync(
                algoritm)
                .ConfigureAwait(false);

            return toReturn;
        }
    }
}