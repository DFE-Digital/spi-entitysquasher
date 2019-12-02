namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.Models;

    /// <summary>
    /// Response object for
    /// <see cref="IGetSquashedEntityProcessor.GetSquashedEntity(GetSquashedEntityRequest)" />.
    /// </summary>
    public class GetSquashedEntityResponse
    {
        /// <summary>
        /// Gets or sets the requested, squashed entity.
        /// </summary>
        public ModelsBase ModelsBase
        {
            get;
            set;
        }
    }
}