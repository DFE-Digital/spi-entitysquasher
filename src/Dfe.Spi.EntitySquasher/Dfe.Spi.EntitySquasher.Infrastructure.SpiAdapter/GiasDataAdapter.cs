using System;
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
    public class GiasDataAdapter : SpiAdapter, IDataAdapter<LearningProvider>, IDataAdapter<ManagementGroup>
    {
        public GiasDataAdapter(EntitySquasherConfiguration configuration, HttpClient httpClient, ISpiExecutionContextManager executionContextManager, ILoggerWrapper logger) 
            : base(configuration.Gias.Url, configuration.Gias.SubscriptionKey, httpClient, executionContextManager, logger)
        {
        }

        async Task<DataAdapterResult<LearningProvider>[]> IDataAdapter<LearningProvider>.GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            DateTime? pointInTime,
            CancellationToken cancellationToken)
        {
            return await GetEntitiesFromApi<LearningProvider>(identifiers, aggregateQueries, fields, live, pointInTime, cancellationToken);
        }

        async Task<DataAdapterResult<ManagementGroup>[]> IDataAdapter<ManagementGroup>.GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            DateTime? pointInTime,
            CancellationToken cancellationToken)
        {
            return await GetEntitiesFromApi<ManagementGroup>(identifiers, aggregateQueries, fields, live, pointInTime, cancellationToken);
        }

        public override string SourceName => "GIAS";
    }
}