namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Abstract base class for task and result classes dealing with
    /// <see cref="IEntityAdapterClient.GetEntityAsync(string, string, IEnumerable{string})" />.
    /// </summary>
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