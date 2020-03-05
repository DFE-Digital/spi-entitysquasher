namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Domain.Models;

    /// <summary>
    /// Describes the operations of the entity adapter invoker.
    /// </summary>
    public interface IEntityAdapterInvoker
    {
        /// <summary>
        /// Invokes entity adapters for an individual
        /// <paramref name="entityReference" />, and returns the results as a
        /// <see cref="InvokeEntityAdaptersResult" /> instance.
        /// </summary>
        /// <param name="algorithm">
        /// The name of the algorithm.
        /// </param>
        /// <param name="entityName">
        /// The name of the entity to return.
        /// </param>
        /// <param name="fields">
        /// A list of fields to include in the response.
        /// </param>
        /// <param name="aggregatesRequest">
        /// An instance of <see cref="AggregatesRequest" />. Optional.
        /// </param>
        /// <param name="entityReference">
        /// An instance of <see cref="EntityReference" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of type <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="InvokeEntityAdaptersResult" />.
        /// </returns>
        Task<InvokeEntityAdaptersResult> InvokeEntityAdaptersAsync(
            string algorithm,
            string entityName,
            IEnumerable<string> fields,
            AggregatesRequest aggregatesRequest,
            EntityReference entityReference,
            CancellationToken cancellationToken);
    }
}