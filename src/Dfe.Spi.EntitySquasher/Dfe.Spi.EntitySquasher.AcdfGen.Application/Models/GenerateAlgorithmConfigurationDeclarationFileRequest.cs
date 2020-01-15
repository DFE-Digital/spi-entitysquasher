namespace Dfe.Spi.EntitySquasher.AcdfGen.Application.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;

    /// <summary>
    /// Request object for
    /// <see cref="IGenerateAlgorithmConfigurationDeclarationFileProcessor.GenerateAlgorithmConfigurationDeclarationFile(GenerateAlgorithmConfigurationDeclarationFileRequest)" />.
    /// </summary>
    public class GenerateAlgorithmConfigurationDeclarationFileRequest
        : ModelsBase
    {
        /// <summary>
        /// Gets or sets a set of adapter names.
        /// </summary>
        public IEnumerable<string> AdapterNames
        {
            get;
            set;
        }
    }
}