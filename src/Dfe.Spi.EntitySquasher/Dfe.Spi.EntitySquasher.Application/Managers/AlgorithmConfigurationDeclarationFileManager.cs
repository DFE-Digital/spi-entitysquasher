namespace Dfe.Spi.EntitySquasher.Application.Managers
{
    using Dfe.Spi.Common.Caching.Definitions.Caches;
    using Dfe.Spi.Common.Caching.Managers;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileManager
        : MemoryCacheManager<string, AlgorithmConfigurationDeclarationFile>, IAlgorithmConfigurationDeclarationFileManager
    {
        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmConfigurationDeclarationFileManager" /> class.
        /// </summary>
        /// <param name="memoryCacheProvider">
        /// An instance of type
        /// <see cref="IMemoryCacheProvider{String, AlgorithmConfigurationDeclarationFile}" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <param name="initialiseCacheItemAsync">
        /// An instance of
        /// <see cref="MemoryCacheManager{String, AlgorithmConfigurationDeclarationFile}.InitialiseCacheItemAsync" />.
        /// </param>
        public AlgorithmConfigurationDeclarationFileManager(
            IMemoryCacheProvider<string, AlgorithmConfigurationDeclarationFile> memoryCacheProvider,
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