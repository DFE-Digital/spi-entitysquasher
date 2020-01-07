namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents the outcome of looking up a particular
    /// <see cref="EntityReference" /> against a set of entity adapters.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AdaptersLookupResult : EntityAdapterResult
    {
        /// <summary>
        /// Gets or sets the looked up <see cref="Spi.Models.ModelsBase" />
        /// instances.
        /// </summary>
        public IEnumerable<Spi.Models.ModelsBase> ModelsBases
        {
            get;
            set;
        }
    }
}