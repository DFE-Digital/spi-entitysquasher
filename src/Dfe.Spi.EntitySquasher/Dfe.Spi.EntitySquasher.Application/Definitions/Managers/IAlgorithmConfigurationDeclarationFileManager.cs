namespace Dfe.Spi.EntitySquasher.Application.Definitions.Managers
{
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="AlgorithmConfigurationDeclarationFile" /> manager.
    /// </summary>
    public interface IAlgorithmConfigurationDeclarationFileManager
        : IManager<string, AlgorithmConfigurationDeclarationFile>
    {
        // Nothing - just inherits what it needs.
    }
}