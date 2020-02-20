namespace Dfe.Spi.EntitySquasher.Application.Models.Result
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Represents the result of a squash operation for a given
    /// <see cref="Models.EntityReference" /> request.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SquashedEntityResult : ModelsBase
    {
        /// <summary>
        /// Gets or sets the originally requested
        /// <see cref="Models.EntityReference" />.
        /// </summary>
        public EntityReference EntityReference
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the requested squashed entity.
        /// </summary>
        public EntityBase SquashedEntity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any
        /// <see cref="Domain.Models.EntityAdapterErrorDetail" />s that
        /// were bubbled up during the initial calling of entity adapters.
        /// </summary>
        public IEnumerable<EntityAdapterErrorDetail> EntityAdapterErrorDetails
        {
            get;
            set;
        }
    }
}