namespace Dfe.Spi.EntitySquasher.Application.Processors.Definitions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;

    /// <summary>
    /// Describes the operations of the get squashed entity processor.
    /// </summary>
    public interface IGetSquashedEntityProcessor
    {
        /// <summary>
        /// The get squashed entity processor entry method.
        /// </summary>
        /// <param name="getSquashedEntityRequest">
        /// An instance of <see cref="GetSquashedEntityRequest" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="GetSquashedEntityResponse" />.
        /// </returns>
        Task<GetSquashedEntityResponse> GetSquashedEntityAsync(
            GetSquashedEntityRequest getSquashedEntityRequest,
            CancellationToken cancellationToken);
    }
}