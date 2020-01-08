namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.Domain;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Result object for
    /// <see cref="IEntityAdapterClient.GetEntityAsync(string, string, IEnumerable{string})" />.
    /// </summary>
    public class GetEntityAsyncResult : GetEntityAsyncBase
    {
        /// <summary>
        /// Gets or sets an instance of type
        /// <see cref="Spi.Models.ModelsBase" />, if the underlying
        /// <see cref="GetEntityAsyncTaskContainer.Task" /> is successful.
        /// Otherwise, this is null.
        /// </summary>
        public Spi.Models.ModelsBase ModelsBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="Domain.EntityAdapterException" />, if the underlying
        /// <see cref="GetEntityAsyncTaskContainer.Task" /> fails in a handled
        /// way.
        /// Otherwise, this is null.
        /// </summary>
        public EntityAdapterException EntityAdapterException
        {
            get;
            set;
        }
    }
}