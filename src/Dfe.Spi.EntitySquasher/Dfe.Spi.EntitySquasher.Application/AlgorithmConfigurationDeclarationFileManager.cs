namespace Dfe.Spi.EntitySquasher.Application
{
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileManager
        : IAlgorithmConfigurationDeclarationFileManager
    {
        private readonly IAlgorithmConfigurationDeclarationFileStorageAdapter algorithmConfigurationDeclarationFileStorageAdapter;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmConfigurationDeclarationFileManager" /> class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileStorageAdapter">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileStorageAdapter" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public AlgorithmConfigurationDeclarationFileManager(
            IAlgorithmConfigurationDeclarationFileStorageAdapter algorithmConfigurationDeclarationFileStorageAdapter,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileStorageAdapter = algorithmConfigurationDeclarationFileStorageAdapter;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<AlgorithmConfigurationDeclarationFile> GetAlgorithmConfigurationDeclarationFileAsync(
            string algoritm)
        {
            AlgorithmConfigurationDeclarationFile toReturn = null;

            // TODO:
            // 1) Check a singleton held in memory for the requested
            //    AlgorithmConfigurationDeclarationFile. If it exists, return
            //    it.
            // 2) Otherwise, pull from the storage, store it in memory, then
            //    return.
            toReturn = await this.algorithmConfigurationDeclarationFileStorageAdapter.GetAlgorithmConfigurationDeclarationFileAsync(
                algoritm)
                .ConfigureAwait(false);

            return toReturn;
        }
    }
}