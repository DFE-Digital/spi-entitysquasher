namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an individual entity reference.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntityReference
    {
        /// <summary>
        /// Gets or sets the adapter record references to squash.
        /// </summary>
        public IEnumerable<AdapterRecordReference> AdapterRecordReferences
        {
            get;
            set;
        }
    }
}