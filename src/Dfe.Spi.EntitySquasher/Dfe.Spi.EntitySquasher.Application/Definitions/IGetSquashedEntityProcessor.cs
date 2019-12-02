namespace Dfe.Spi.EntitySquasher.Application.Definitions
{
    using Dfe.Spi.EntitySquasher.Application.Models;

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
        /// <returns>
        /// An instance of <see cref="GetSquashedEntityResponse" />.
        /// </returns>
        GetSquashedEntityResponse GetSquashedEntity(
            GetSquashedEntityRequest getSquashedEntityRequest);
    }
}