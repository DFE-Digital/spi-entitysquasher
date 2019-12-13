namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.Models;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClient" />.
    /// </summary>
    public class EntityAdapterClient : IEntityAdapterClient
    {
        private readonly Uri baseUrl;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClient" /> class.
        /// </summary>
        /// <param name="baseUrl">
        /// The base URL of the entity adapter.
        /// </param>
        public EntityAdapterClient(Uri baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        /// <inheritdoc />
        public Task<ModelsBase> GetEntityAsync(
            string entityName,
            string id,
            IEnumerable<string> fields)
        {
            // TODO: Maybe later. This is going to be tricky to do with no
            //       actual API to call.
            throw new NotImplementedException();
        }
    }
}