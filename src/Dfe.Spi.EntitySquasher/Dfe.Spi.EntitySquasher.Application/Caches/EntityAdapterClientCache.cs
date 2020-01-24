namespace Dfe.Spi.EntitySquasher.Application.Caches
{
    using Dfe.Spi.Common.Caching;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientCache" />.
    /// </summary>
    public class EntityAdapterClientCache
        : CacheProvider, IEntityAdapterClientCache
    {
        // Nothing - inherits all it needs for now.
    }
}