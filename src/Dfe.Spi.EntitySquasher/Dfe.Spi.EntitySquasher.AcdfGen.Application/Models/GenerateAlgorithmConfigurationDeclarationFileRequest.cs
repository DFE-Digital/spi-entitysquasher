namespace Dfe.Spi.EntitySquasher.AcdfGen.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Request object for
    /// <see cref="IGenerateAlgorithmConfigurationDeclarationFileProcessor.GenerateAlgorithmConfigurationDeclarationFile(GenerateAlgorithmConfigurationDeclarationFileRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GenerateAlgorithmConfigurationDeclarationFileRequest
        : ModelsBase
    {
        /// <summary>
        /// Gets or sets the filename to save the
        /// <see cref="AlgorithmConfigurationDeclarationFile" /> to the
        /// underlying storage.
        /// </summary>
        public string Filename
        {
            get;
            set;
        }
    }
}