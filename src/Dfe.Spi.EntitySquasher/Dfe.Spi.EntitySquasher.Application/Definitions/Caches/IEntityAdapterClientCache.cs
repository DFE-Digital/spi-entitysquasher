namespace Dfe.Spi.EntitySquasher.Application.Definitions.Caches
{
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Describes the operations of the <see cref="IEntityAdapterClient" />
    /// cache.
    /// </summary>
    public interface IEntityAdapterClientCache : ICache<IEntityAdapterClient>
    {
        // Nothing, inherits what it needs.
    }
}