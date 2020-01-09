namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// A container class for tasks returned from
    /// <see cref="IEntityAdapterClient.GetEntityAsync(string, string, IEnumerable{string})" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetEntityAsyncTaskContainer : GetEntityAsyncBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="System.Threading.Tasks.Task "/>.
        /// </summary>
        public Task<Spi.Models.ModelsBase> Task
        {
            get;
            set;
        }
    }
}