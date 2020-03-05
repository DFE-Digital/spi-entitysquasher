namespace Dfe.Spi.EntitySquasher.Domain.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a request for a set of aggregates.
    /// </summary>
    public class AggregatesRequest : ModelsBase
    {
        /// <summary>
        /// Gets or sets the aggregate queries.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Usage",
            "CA2227",
            Justification = "This is a DTO.")]
        public Dictionary<string, AggregateQuery> AggregateQueries
        {
            get;
            set;
        }
    }
}