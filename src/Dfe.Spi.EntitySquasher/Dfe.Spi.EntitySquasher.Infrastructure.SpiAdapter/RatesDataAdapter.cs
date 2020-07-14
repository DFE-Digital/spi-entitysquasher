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
    public class RatesDataAdapter : SpiAdapter, IDataAdapter<LearningProviderRates>, IDataAdapter<ManagementGroupRates>
    {
        public RatesDataAdapter(EntitySquasherConfiguration configuration, HttpClient httpClient, ISpiExecutionContextManager executionContextManager, ILoggerWrapper logger) 
            : base(configuration.Rates.Url, configuration.Rates.SubscriptionKey, httpClient, executionContextManager, logger)
        {
        }

        async Task<DataAdapterResult<LearningProviderRates>[]> IDataAdapter<LearningProviderRates>.GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            CancellationToken cancellationToken)
        {
            return await GetEntitiesFromApi<LearningProviderRates>(identifiers, aggregateQueries, fields, live, cancellationToken);
        }

        async Task<DataAdapterResult<ManagementGroupRates>[]> IDataAdapter<ManagementGroupRates>.GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            CancellationToken cancellationToken)
        {
            return await GetEntitiesFromApi<ManagementGroupRates>(identifiers, aggregateQueries, fields, live, cancellationToken);
        }

        public override string SourceName => "Rates";
    }
}