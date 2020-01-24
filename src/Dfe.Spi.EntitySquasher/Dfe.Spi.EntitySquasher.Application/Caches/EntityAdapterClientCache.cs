namespace Dfe.Spi.EntitySquasher.Application.Caches
{
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.Common.Caching;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientCache" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntityAdapterClientCache
        : CacheProvider, IEntityAdapterClientCache
    {
        // Nothing - inherits all it needs for now.
    }
}