namespace Dfe.Spi.EntitySquasher.Application.Definitions.Factories
{
    using Dfe.Spi.Common.Caching.Definitions.Factories.Managers;
    using Dfe.Spi.Common.Caching.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Describes the operations of the <see cref="IEntityAdapterClient" />
    /// <see cref="ICacheManager"/> manager factory.
    /// </summary>
    public interface IEntityAdapterClientCacheManagerFactory
        : ICacheManagerFactory
    {
        // Nothing.
    }
}