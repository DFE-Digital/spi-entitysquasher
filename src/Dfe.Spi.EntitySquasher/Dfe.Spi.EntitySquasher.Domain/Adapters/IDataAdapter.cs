using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;

namespace Dfe.Spi.EntitySquasher.Domain.Adapters
{
    public interface IDataAdapter
    {
        string SourceName { get; }
    }

    public interface IDataAdapter<T> : IDataAdapter
    {
        Task<DataAdapterResult<T>[]> GetEntitiesAsync(
            string[] identifiers, 
            Dictionary<string, AggregateQuery> aggregateQueries, 
            string[] fields, 
            bool live, 
            DateTime? pointInTime,
            CancellationToken cancellationToken);
    }
}