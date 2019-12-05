namespace Dfe.Spi.EntitySquasher.Domain.Definitions
{
    using Dfe.Spi.EntitySquasher.Domain.Models.Adcf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="AlgorithmDeclarationConfigurationFile" /> manager.
    /// </summary>
    public interface IAlgorithmDeclarationConfigurationFileManager
    {
        /// <summary>
        /// Returns a <see cref="AlgorithmDeclarationConfigurationFile" /> for
        /// a given <paramref name="algoritm" />.
        /// </summary>
        /// <param name="algoritm">
        /// The algorithm.
        /// </param>
        /// <returns>
        /// An instance of
        /// <see cref="AlgorithmDeclarationConfigurationFile" />, unless not
        /// found for the given <paramref name="algoritm" />, in which case
        /// null.
        /// </returns>
        AlgorithmDeclarationConfigurationFile GetAlgorithmDeclarationConfigurationFile(
            string algoritm);
    }
}