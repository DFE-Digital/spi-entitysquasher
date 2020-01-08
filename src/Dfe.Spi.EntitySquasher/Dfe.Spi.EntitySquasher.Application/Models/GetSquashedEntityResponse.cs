namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;

    /// <summary>
    /// Response object for
    /// <see cref="IGetSquashedEntityProcessor.GetSquashedEntityAsync(GetSquashedEntityRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetSquashedEntityResponse : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets a set of <see cref="SquashedEntityResult" />
        /// instances.
        /// </summary>
        public IEnumerable<SquashedEntityResult> SquashedEntityResults
        {
            get;
            set;
        }
    }
}