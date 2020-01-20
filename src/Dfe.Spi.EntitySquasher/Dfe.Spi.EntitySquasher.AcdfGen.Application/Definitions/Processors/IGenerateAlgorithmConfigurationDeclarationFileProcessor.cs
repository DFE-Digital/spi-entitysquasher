namespace Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors
{
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the generate
    /// <see cref="AlgorithmConfigurationDeclarationFile" /> processor.
    /// </summary>
    public interface IGenerateAlgorithmConfigurationDeclarationFileProcessor
    {
        /// <summary>
        /// Generates an <see cref="AlgorithmConfigurationDeclarationFile" />
        /// based on the inputs and the current state of the
        /// <see cref="Spi.Models" /> library and writes it to disk.
        /// </summary>
        /// <param name="generateAlgorithmConfigurationDeclarationFileRequest">
        /// An instance of
        /// <see cref="GenerateAlgorithmConfigurationDeclarationFileRequest" />.
        /// </param>
        /// <returns>
        /// An instance of
        /// <see cref="GenerateAlgorithmConfigurationDeclarationFileResponse" />.
        /// </returns>
        GenerateAlgorithmConfigurationDeclarationFileResponse GenerateAlgorithmConfigurationDeclarationFile(
            GenerateAlgorithmConfigurationDeclarationFileRequest generateAlgorithmConfigurationDeclarationFileRequest);
    }
}