using System.Collections.Generic;

namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter.Models
{
    using Dfe.Spi.EntitySquasher.Domain.Models;

    /// <summary>
    /// Model for querying adapters for multiple entities.
    /// </summary>
    public class BatchGetEntitiesRequest
    {
        /// <summary>
        /// Gets or sets the identifiers to request from adapter.
        /// </summary>
        public string[] Identifiers { get; set; }

        /// <summary>
        /// Gets or sets the fields to request from the adapter.
        /// </summary>
        public string[] Fields { get; set; }

        /// <summary>
        /// Gets or sets the aggregations to request from the adapter.
        /// </summary>
        public Dictionary<string, AggregateQuery> AggregateQueries { get; set; }
    }
}