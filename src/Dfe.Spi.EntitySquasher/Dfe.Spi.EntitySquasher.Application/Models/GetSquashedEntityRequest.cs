namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.Application.Definitions;

    /// <summary>
    /// Request object for
    /// <see cref="IGetSquashedEntityProcessor.GetSquashedEntityAsync(GetSquashedEntityRequest)" />.
    /// </summary>
    public class GetSquashedEntityRequest : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets the adapter record references to squash.
        /// </summary>
        public IEnumerable<AdapterRecordReference> AdapterRecordReferences
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the algorithm.
        /// Optional: if left blank, then the default algorithm will be used.
        /// </summary>
        public string Algorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the entity to return.
        /// </summary>
        public string EntityName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of fields to include in the response.
        /// </summary>
        public IEnumerable<string> Fields
        {
            get;
            set;
        }
    }
}