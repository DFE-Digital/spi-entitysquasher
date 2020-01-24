namespace Dfe.Spi.EntitySquasher.Application.Caches
{
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.Common.Caching;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Implements <see cref="IAlgorithmConfigurationDeclarationFileCache" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AlgorithmConfigurationDeclarationFileCache
        : CacheProvider, IAlgorithmConfigurationDeclarationFileCache
    {
        // Nothing - inherits all it needs for now.
    }
}