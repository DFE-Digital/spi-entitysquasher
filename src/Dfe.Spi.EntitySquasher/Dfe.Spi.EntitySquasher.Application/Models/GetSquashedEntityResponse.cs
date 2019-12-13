namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;

    /// <summary>
    /// Response object for
    /// <see cref="IGetSquashedEntityProcessor.GetSquashedEntityAsync(GetSquashedEntityRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetSquashedEntityResponse
        : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets the requested, squashed entities.
        /// </summary>
        public IEnumerable<Spi.Models.ModelsBase> Entities
        {
            get;
            set;
        }
    }
}