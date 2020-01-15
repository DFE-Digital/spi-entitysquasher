namespace Dfe.Spi.EntitySquasher.AcdfGen.Domain.Definitions
{
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the generated
    /// <see cref="AlgorithmConfigurationDeclarationFile" /> repository.
    /// </summary>
    public interface IGeneratedAlgorithmConfigurationDeclarationFileRepository
    {
        /// <summary>
        /// Saves an <see cref="AlgorithmConfigurationDeclarationFile" /> to
        /// the underlying storage.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFile">
        /// An instance of
        /// <see cref="AlgorithmConfigurationDeclarationFile" />.
        /// </param>
        /// <returns>
        /// The full path to the file saved, as a <see cref="string" /> value.
        /// </returns>
        string Save(
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile);
    }
}