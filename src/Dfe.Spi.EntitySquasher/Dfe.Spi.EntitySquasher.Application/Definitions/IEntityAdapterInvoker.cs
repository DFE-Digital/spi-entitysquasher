namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Application.Models;

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
        /// <param name="entityReference">
        /// An instance of <see cref="EntityReference" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="InvokeEntityAdaptersResult" />.
        /// </returns>
        Task<InvokeEntityAdaptersResult> InvokeEntityAdapters(
            string algorithm,
            string entityName,
            IEnumerable<string> fields,
            EntityReference entityReference);
    }
}