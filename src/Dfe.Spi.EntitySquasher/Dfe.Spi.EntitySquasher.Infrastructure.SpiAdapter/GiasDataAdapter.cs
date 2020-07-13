using System.Collections.Generic;
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
    public class GiasDataAdapter : SpiAdapter, IDataAdapter<LearningProvider>, IDataAdapter<ManagementGroup>
    {
        public GiasDataAdapter(EntitySquasherConfiguration configuration, ISpiExecutionContextManager executionContextManager, ILoggerWrapper logger) 
            : base(configuration.Gias.Url, configuration.Gias.SubscriptionKey, executionContextManager, logger)
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

        async Task<DataAdapterResult<ManagementGroup>[]> IDataAdapter<ManagementGroup>.GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            CancellationToken cancellationToken)
        {
            return await GetEntitiesFromApi<ManagementGroup>(identifiers, aggregateQueries, fields, live, cancellationToken);
        }

        public override string SourceName => "GIAS";
    }
}