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
    public class UkrlpDataAdapter : SpiAdapter, IDataAdapter<LearningProvider>
    {
        public UkrlpDataAdapter(EntitySquasherConfiguration configuration, HttpClient httpClient, ISpiExecutionContextManager executionContextManager, ILoggerWrapper logger) 
            : base(configuration.Ukrlp.Url, configuration.Ukrlp.SubscriptionKey, httpClient, executionContextManager, logger)
        {
        }

        async Task<DataAdapterResult<LearningProvider>[]> IDataAdapter<LearningProvider>.GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            CancellationToken cancellationToken)
        {
            return await GetEntitiesFromApi<LearningProvider>(identifiers, aggregateQueries, fields, live, cancellationToken);
        }

        public override string SourceName => "UKRLP";
    }
}