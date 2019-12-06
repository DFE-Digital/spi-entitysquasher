namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="AlgorithmConfigurationDeclarationFile" /> cache.
    /// </summary>
    public interface IAlgorithmConfigurationDeclarationFileCache
    {
        /// <summary>
        /// Adds a <see cref="AlgorithmConfigurationDeclarationFile" />
        /// instance to the cache.
        /// </summary>
        /// <param name="algorithm">
        /// The algorithm.
        /// </param>
        /// <param name="algorithmConfigurationDeclarationFile">
        /// The <see cref="AlgorithmConfigurationDeclarationFile" /> instance
        /// to cache.
        /// </param>
        void AddAlgorithmConfigurationDeclarationFile(
            string algorithm,
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile);

        /// <summary>
        /// Gets a <see cref="AlgorithmConfigurationDeclarationFile" />
        /// instance from the cache. Will return null if not present in the
        /// cache.
        /// </summary>
        /// <param name="algorithm">
        /// The algorithm.
        /// </param>
        /// <returns>
        /// An instance of
        /// <see cref="AlgorithmConfigurationDeclarationFile" />, unless an
        /// instance was not found in the cache for the given
        /// <paramref name="algorithm" />, in which case null.
        /// </returns>
        AlgorithmConfigurationDeclarationFile GetAlgorithmConfigurationDeclarationFile(
            string algorithm);
    }
}