namespace Dfe.Spi.EntitySquasher.Domain.Definitions.Factories
{
    using System;

    /// <summary>
    /// Describes the operations of the <see cref="IEntityAdapterClient" />
    /// factory.
    /// </summary>
    public interface IEntityAdapterClientFactory
    {
        /// <summary>
        /// Creates an instance of type <see cref="IEntityAdapterClient" />.
        /// </summary>
        /// <param name="baseUrl">
        /// The base URL of the entity adapter.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IEntityAdapterClient" />.
        /// </returns>
        IEntityAdapterClient Create(Uri baseUrl);
    }
}