namespace Dfe.Spi.EntitySquasher.Application.Definitions.Managers
{
    using Dfe.Spi.Common.Caching.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Describes the operations of the <see cref="IEntityAdapterClient" />
    /// manager.
    /// </summary>
    public interface IEntityAdapterClientManager
        : IMemoryCacheManager<EntityAdapterClientKey, IEntityAdapterClient>
    {
        // Nothing - just inherits what it needs.
    }
}