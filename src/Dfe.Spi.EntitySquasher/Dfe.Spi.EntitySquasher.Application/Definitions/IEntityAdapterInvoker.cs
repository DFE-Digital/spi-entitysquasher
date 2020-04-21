using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.EntitySquasher.Application.Models;
using Dfe.Spi.EntitySquasher.Application.Models.Result;
using Dfe.Spi.EntitySquasher.Domain.Models;

namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    /// <summary>
    /// Interface to calling entity adapters.
    /// </summary>
    public interface IEntityAdapterInvoker
    {
        /// <summary>
        /// Calls all relevant adapters to get entity data.
        /// </summary>
        /// <param name="entityName">
        /// Name of the entity.
        /// </param>
        /// <param name="entityReferences">
        /// The entity references requested.
        /// </param>
        /// <param name="fields">
        /// The fields requested.
        /// </param>
        /// <param name="aggregatesRequest">
        /// The aggregations requested.
        /// </param>
        /// <param name="cancellationToken">
        /// Task cancellation token.
        /// </param>
        /// <returns>
        /// A Dictionary of results for each adapter reference in <paramref name="entityReferences"/>
        /// </returns>
        Task<Dictionary<AdapterRecordReference, GetEntityAsyncResult>> GetResultsFromAdaptersAsync(
            string entityName,
            EntityReference[] entityReferences,
            string[] fields,
            AggregatesRequest aggregatesRequest,
            CancellationToken cancellationToken);
    }
}