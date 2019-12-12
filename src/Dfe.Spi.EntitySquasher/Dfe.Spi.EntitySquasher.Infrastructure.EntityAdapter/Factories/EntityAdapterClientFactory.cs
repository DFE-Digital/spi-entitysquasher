namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter.Factories
{
    using System;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientFactory" />.
    /// </summary>
    public class EntityAdapterClientFactory : IEntityAdapterClientFactory
    {
        /// <inheritdoc />
        public IEntityAdapterClient Create(Uri baseUrl)
        {
            EntityAdapterClient toReturn = new EntityAdapterClient(baseUrl);

            return toReturn;
        }
    }
}