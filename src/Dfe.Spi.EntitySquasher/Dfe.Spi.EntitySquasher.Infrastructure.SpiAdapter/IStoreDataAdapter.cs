using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Context.Definitions;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.EntitySquasher.Domain.Adapters;
using Dfe.Spi.EntitySquasher.Domain.Configuration;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Dfe.Spi.Models.Entities;

namespace Dfe.Spi.EntitySquasher.Infrastructure.SpiAdapter
{
    public class IStoreDataAdapter : SpiAdapter, IDataAdapter<Census>
    {
        public IStoreDataAdapter(EntitySquasherConfiguration configuration, HttpClient httpClient, ISpiExecutionContextManager executionContextManager, ILoggerWrapper logger) 
            : base(configuration.IStore.Url, configuration.IStore.SubscriptionKey, httpClient, executionContextManager, logger)
        {
        }

        async Task<DataAdapterResult<Census>[]> IDataAdapter<Census>.GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            CancellationToken cancellationToken)
        {
            if (aggregateQueries != null)
            {
                foreach (var aggregateQuery in aggregateQueries.Values)
                {
                    aggregateQuery.AggregateType = aggregateQuery.AggregateType ?? "Count";
                }
            }

            return await GetEntitiesFromApi<Census>(identifiers, aggregateQueries, fields, live, cancellationToken);
        }

        public override string SourceName => "IStore";
    }
}