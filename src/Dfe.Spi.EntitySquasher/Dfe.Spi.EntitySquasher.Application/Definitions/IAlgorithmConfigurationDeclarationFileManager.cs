namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="AlgorithmConfigurationDeclarationFile" /> manager.
    /// </summary>
    public interface IAlgorithmConfigurationDeclarationFileManager
    {
        /// <summary>
        /// Returns a <see cref="AlgorithmConfigurationDeclarationFile" /> for
        /// a given <paramref name="algoritm" />.
        /// </summary>
        /// <param name="algoritm">
        /// The algorithm.
        /// </param>
        /// <returns>
        /// An instance of
        /// <see cref="AlgorithmConfigurationDeclarationFile" />, unless not
        /// found for the given <paramref name="algoritm" />, in which case
        /// null.
        /// </returns>
        Task<AlgorithmConfigurationDeclarationFile> GetAlgorithmConfigurationDeclarationFileAsync(
            string algoritm);
    }
}