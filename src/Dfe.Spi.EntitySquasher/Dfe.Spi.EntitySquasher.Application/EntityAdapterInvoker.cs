

namespace Dfe.Spi.EntitySquasher.Application
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Domain;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    
    /// <summary>
    /// Implements <see cref="IEntityAdapterInvoker" />.
    /// </summary>
    public class EntityAdapterInvoker : IEntityAdapterInvoker
    {
        private readonly IEntityAdapterClientFactory entityAdapterClientFactory;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterInvoker" /> class.
        /// </summary>
        /// <param name="entityAdapterClientFactory">
        /// An instance of type <see cref="IEntityAdapterClientFactory" />.
        /// </param>
        public EntityAdapterInvoker(
            IEntityAdapterClientFactory entityAdapterClientFactory)
        {
            this.entityAdapterClientFactory = entityAdapterClientFactory;
        }
        
        /// <inheritdoc />
        public async Task<Dictionary<AdapterRecordReference, GetEntityAsyncResult>> GetResultsFromAdaptersAsync(
            string entityName, 
            EntityReference[] entityReferences, 
            string[] fields, 
            AggregatesRequest aggregatesRequest,
            bool live,
            CancellationToken cancellationToken)
        {
            var results = new Dictionary<AdapterRecordReference, GetEntityAsyncResult>();

            var adapterReferences = entityReferences
                .SelectMany(x => x.AdapterRecordReferences)
                .Distinct()
                .GroupBy(x => x.Source)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var tasks = adapterReferences.Keys
                .Select(adapterName =>
                    GetResultsFromAdapterAsync(
                        adapterName,
                        entityName,
                        adapterReferences[adapterName],
                        fields,
                        aggregatesRequest,
                        live,
                        cancellationToken));

            var adapterResults = await Task.WhenAll(tasks);
            foreach (var adapterResult in adapterResults)
            {
                foreach (var resultReference in adapterResult.Keys)
                {
                    results.Add(resultReference, adapterResult[resultReference]);
                }
            }

            return results;
        }
        
        

        private async Task<Dictionary<AdapterRecordReference, GetEntityAsyncResult>> GetResultsFromAdapterAsync(
            string adapterName, 
            string entityName, 
            AdapterRecordReference[] references, 
            string[] fields,
            AggregatesRequest aggregatesRequest,
            bool live,
            CancellationToken cancellationToken)
        {
            var results = new Dictionary<AdapterRecordReference, GetEntityAsyncResult>();
            
            var ids = references
                .Select(x => x.Id)
                .ToArray();

            var adapterClient = this.entityAdapterClientFactory.Create(adapterName);

            try
            {
                var adapterResults = await adapterClient.GetEntitiesAsync(
                    entityName,
                    ids,
                    fields,
                    aggregatesRequest,
                    live,
                    cancellationToken);

                for (var i = 0; i < references.Length; i++)
                {
                    var result = new GetEntityAsyncResult
                    {
                        EntityBase = adapterResults[i],
                        AdapterRecordReference = references[i],
                    };
                    if (result.EntityBase == null)
                    {
                        result.EntityAdapterException = new EntityAdapterException
                        {
                            HttpStatusCode = HttpStatusCode.NotFound,
                        };
                    }

                    results.Add(references[i], result);
                }
            }
            catch (EntityAdapterException ex)
            {
                foreach (var reference in references)
                {
                    results.Add(reference, new GetEntityAsyncResult
                    {
                        AdapterRecordReference = reference,
                        EntityAdapterException = ex,
                    });
                }
            }

            return results;
        }
    }
}