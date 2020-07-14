using Dfe.Spi.Common.Models;

namespace Dfe.Spi.EntitySquasher.Domain.RequestResponseModels
{
    public class AggregateQuery
    {
        public DataFilter[] DataFilters { get; set; }
        public string AggregateType { get; set; }
    }
}