namespace Dfe.Spi.EntitySquasher.Application.Caches
{
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientCache" />.
    /// </summary>
    public class EntityAdapterClientCache
        : Cache<EntityAdapterClientKey, IEntityAdapterClient>, IEntityAdapterClientCache
    {
        // Nothing - inherits all it needs for now.
    }
}