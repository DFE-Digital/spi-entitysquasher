namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.Models;

    /// <summary>
    /// Describes the operations of the result squasher.
    /// </summary>
    public interface IResultSquasher
    {
        /// <summary>
        /// Squashes more than one
        /// <see cref="GetEntityAsyncResult.ModelsBase" /> instance together,
        /// to produce a single <see cref="ModelsBase" /> instance, according
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
        /// <see cref="ModelsBase" />s to squash together.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ModelsBase" />.
        /// </returns>
        Task<ModelsBase> SquashAsync(
            string algorithm,
            string entityName,
            IEnumerable<GetEntityAsyncResult> toSquash);
    }
}