namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;

    /// <summary>
    /// Abstract base class for task and result classes dealing with
    /// <see cref="IEntityAdapterClient.GetEntityAsync(string, string, IEnumerable{string}, AggregatesRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class GetEntityAsyncBase : ModelsBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="Models.AdapterRecordReference" />.
        /// </summary>
        public AdapterRecordReference AdapterRecordReference
        {
            get;
            set;
        }
    }
}