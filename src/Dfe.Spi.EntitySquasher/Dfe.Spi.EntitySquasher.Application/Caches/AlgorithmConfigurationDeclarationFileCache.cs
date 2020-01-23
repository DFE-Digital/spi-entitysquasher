namespace Dfe.Spi.EntitySquasher.Application.Caches
{
    using Dfe.Spi.Common.Caching.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements <see cref="IAlgorithmConfigurationDeclarationFileCache" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileCache
        : MemoryCacheProvider<string, AlgorithmConfigurationDeclarationFile>, IAlgorithmConfigurationDeclarationFileCache
    {
        // Nothing - inherits all it needs for now.
    }
}