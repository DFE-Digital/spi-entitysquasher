using System.Collections.Generic;

namespace Dfe.Spi.EntitySquasher.Domain.RequestResponseModels
{
    public class AggregatesRequest
    {
        public Dictionary<string, AggregateQuery> AggregateQueries { get; set; }
    }
}