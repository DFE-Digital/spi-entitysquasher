namespace Dfe.Spi.EntitySquasher.Application.Models.Result
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Domain;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Result object for
    /// <see cref="IEntityAdapterClient.GetEntityAsync(string, string, IEnumerable{string}, AggregatesRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetEntityAsyncResult : GetEntityAsyncBase
    {
        /// <summary>
        /// Gets or sets an instance of type
        /// <see cref="Spi.Models.Entities.EntityBase" />, if the underlying
        /// <see cref="GetEntityAsyncTaskContainer.Task" /> is successful.
        /// Otherwise, this is null.
        /// </summary>
        public EntityBase EntityBase
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