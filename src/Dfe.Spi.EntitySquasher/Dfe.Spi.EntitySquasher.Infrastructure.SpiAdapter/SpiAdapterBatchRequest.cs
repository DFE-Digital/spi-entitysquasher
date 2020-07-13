using System.Collections.Generic;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;

namespace Dfe.Spi.EntitySquasher.Infrastructure.SpiAdapter
{
    public class SpiAdapterBatchRequest
    {
        public string[] Identifiers { get; set; }
        public string[] Fields { get; set; }
        public bool Live { get; set; }
        public Dictionary<string, AggregateQuery> AggregateQueries { get; set; }
    }
}