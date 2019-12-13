namespace Dfe.Spi.EntitySquasher.Application.Definitions.Caches
{
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="AlgorithmConfigurationDeclarationFile" /> cache.
    /// </summary>
    public interface IAlgorithmConfigurationDeclarationFileCache
        : ICache<string, AlgorithmConfigurationDeclarationFile>
    {
        // Nothing, inherits what it needs.
    }
}