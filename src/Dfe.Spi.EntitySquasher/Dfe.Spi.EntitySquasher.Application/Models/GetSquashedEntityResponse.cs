namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.Models;

    /// <summary>
    /// Response object for
    /// <see cref="IGetSquashedEntityProcessor.GetSquashedEntityAsync(GetSquashedEntityRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetSquashedEntityResponse : RequestResponseBase
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