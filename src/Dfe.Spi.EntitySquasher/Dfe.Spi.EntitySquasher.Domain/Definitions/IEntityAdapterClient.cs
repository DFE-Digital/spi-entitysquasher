namespace Dfe.Spi.EntitySquasher.Domain.Definitions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dfe.Spi.Models;

    /// <summary>
    /// Describes the operations of the entity adapter client.
    /// </summary>
    public interface IEntityAdapterClient
    {
        /// <summary>
        /// Gets an entity from an entity adapter.
        /// </summary>
        /// <param name="entityName">
        /// The name of the entity to return.
        /// </param>
        /// <param name="id">
        /// The id of the entity.
        /// </param>
        /// <param name="fields">
        /// The fields to return from the adapter.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="ModelsBase" />.
        /// </returns>
        Task<ModelsBase> GetEntityAsync(
            string entityName,
            string id,
            IEnumerable<string> fields);
    }
}