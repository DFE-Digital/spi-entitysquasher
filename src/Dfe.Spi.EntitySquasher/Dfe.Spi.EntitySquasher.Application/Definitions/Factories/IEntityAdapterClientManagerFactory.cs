namespace Dfe.Spi.EntitySquasher.Application.Definitions.Factories
{
    using Dfe.Spi.Common.Caching.Definitions.Factories.Managers;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="IEntityAdapterClientManager" /> factory.
    /// </summary>
    public interface IEntityAdapterClientManagerFactory
        : ICacheManagerFactory<EntityAdapterClientKey, IEntityAdapterClient>
    {
        /// <summary>
        /// Creates an instance of type
        /// <see cref="IEntityAdapterClientManager" />.
        /// </summary>
        /// <returns>
        /// An instance of type <see cref="IEntityAdapterClientManager" />.
        /// </returns>
        IEntityAdapterClientManager Create();
    }
}