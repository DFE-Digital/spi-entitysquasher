namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// A container class for tasks returned from
    /// <see cref="IEntityAdapterClient.GetEntityAsync(string, string, IEnumerable{string}, AggregatesRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetEntityAsyncTaskContainer : GetEntityAsyncBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="System.Threading.Tasks.Task "/>.
        /// </summary>
        public Task<EntityBase> Task
        {
            get;
            set;
        }
    }
}