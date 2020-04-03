namespace Dfe.Spi.EntitySquasher.Domain.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Represents an individual aggregate query.
    /// </summary>
    public class AggregateQuery : ModelsBase
    {
        private const AggregateType DefaultAggregateType = AggregateType.Count;

        private AggregateType? aggregateType;

        /// <summary>
        /// Gets or sets a set of <see cref="DataFilter" /> instances.
        /// </summary>
        public IEnumerable<DataFilter> DataFilters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the aggregate type. If not explicitly set, then
        /// <see cref="AggregateType.Count" /> is returned.
        /// </summary>
        public AggregateType AggregateType
        {
            get
            {
                AggregateType toReturn;

                if (this.aggregateType.HasValue)
                {
                    toReturn = this.aggregateType.Value;
                }
                else
                {
                    toReturn = DefaultAggregateType;
                }

                return toReturn;
            }

            set
            {
                this.aggregateType = value;
            }
        }
    }
}