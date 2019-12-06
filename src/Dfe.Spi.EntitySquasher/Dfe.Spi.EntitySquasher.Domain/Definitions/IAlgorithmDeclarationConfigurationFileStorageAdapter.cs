namespace Dfe.Spi.EntitySquasher.Domain.Definitions
{
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Domain.Models.Adcf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="AlgorithmDeclarationConfigurationFile" /> storage adapter.
    /// </summary>
    public interface IAlgorithmDeclarationConfigurationFileStorageAdapter
    {
        /// <summary>
        /// Gets from storage the specified
        /// <see cref="AlgorithmDeclarationConfigurationFile" />, based on the
        /// supplied <paramref name="algorithm" />.
        /// </summary>
        /// <param name="algorithm">
        /// The algorithm.
        /// </param>
        /// <returns>
        /// An instance of
        /// <see cref="AlgorithmDeclarationConfigurationFile" />.
        /// </returns>
        Task<AlgorithmDeclarationConfigurationFile> GetAlgorithmDeclarationConfigurationFileAsync(
            string algorithm);
    }
}