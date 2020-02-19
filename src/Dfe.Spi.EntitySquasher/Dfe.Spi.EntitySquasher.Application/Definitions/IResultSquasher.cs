namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.Models;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Describes the operations of the result squasher.
    /// </summary>
    public interface IResultSquasher
    {
        /// <summary>
        /// Squashes more than one
        /// <see cref="GetEntityAsyncResult.EntityBase" /> instance together,
        /// to produce a single <see cref="EntityBase" /> instance, according
        /// to the supplied <paramref name="algorithm" />.
        /// </summary>
        /// <param name="algorithm">
        /// The name of the algorithm.
        /// </param>
        /// <param name="entityName">
        /// The name of the entity to return.
        /// </param>
        /// <param name="toSquash">
        /// The <see cref="GetEntityAsyncResult" /> instances, containing the
        /// <see cref="EntityBase" />s to squash together.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="EntityBase" />.
        /// </returns>
        Task<EntityBase> SquashAsync(
            string algorithm,
            string entityName,
            IEnumerable<GetEntityAsyncResult> toSquash,
            CancellationToken cancellationToken);
    }
}