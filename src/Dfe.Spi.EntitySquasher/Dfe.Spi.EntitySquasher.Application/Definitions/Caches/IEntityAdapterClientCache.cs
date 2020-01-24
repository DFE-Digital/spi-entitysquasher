namespace Dfe.Spi.EntitySquasher.Application.Definitions.Caches
{
    using Dfe.Spi.Common.Caching.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Describes the operations of the <see cref="IEntityAdapterClient" />
    /// cache.
    /// </summary>
    public interface IEntityAdapterClientCache : ICacheProvider
    {
        // Nothing, inherits what it needs.
    }
}