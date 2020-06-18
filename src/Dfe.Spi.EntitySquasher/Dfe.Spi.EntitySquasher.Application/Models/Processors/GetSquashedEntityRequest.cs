namespace Dfe.Spi.EntitySquasher.Application.Models.Processors
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;

    /// <summary>
    /// Request object for
    /// <see cref="IGetSquashedEntityProcessor.GetSquashedEntityAsync(GetSquashedEntityRequest, CancellationToken)" />.
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
        public string[] Fields
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="Domain.Models.AggregatesRequest" />. Optional.
        /// </summary>
        public AggregatesRequest AggregatesRequest
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether adapters should query live or cache (where available)
        /// </summary>
        public bool Live
        {
            get;
            set;
        }
    }
}