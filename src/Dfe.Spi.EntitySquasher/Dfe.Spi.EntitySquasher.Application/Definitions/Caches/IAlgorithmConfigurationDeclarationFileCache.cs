namespace Dfe.Spi.EntitySquasher.Application.Definitions.Caches
{
    using Dfe.Spi.Common.Caching.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="AlgorithmConfigurationDeclarationFile" /> cache.
    /// </summary>
    public interface IAlgorithmConfigurationDeclarationFileCache
        : IMemoryCacheProvider<string, AlgorithmConfigurationDeclarationFile>
    {
        // Nothing, inherits what it needs.
    }
}