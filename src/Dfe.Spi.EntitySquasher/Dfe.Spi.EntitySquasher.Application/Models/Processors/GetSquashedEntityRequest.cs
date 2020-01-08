namespace Dfe.Spi.EntitySquasher.Application.Models.Processors
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;

    /// <summary>
    /// Request object for
    /// <see cref="IGetSquashedEntityProcessor.GetSquashedEntityAsync(GetSquashedEntityRequest)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetSquashedEntityRequest : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets a set of <see cref="EntityReference" /> instances.
        /// </summary>
        public IEnumerable<EntityReference> EntityReferences
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the algorithm.
        /// Optional: if left blank, then the default algorithm will be used.
        /// </summary>
        public string Algorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the entity to return.
        /// </summary>
        public string EntityName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of fields to include in the response.
        /// </summary>
        public IEnumerable<string> Fields
        {
            get;
            set;
        }
    }
}