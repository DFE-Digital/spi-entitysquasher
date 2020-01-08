namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.Domain;

    /// <summary>
    /// Represents the result of a squash operation for a given
    /// <see cref="Models.EntityReference" /> request.
    /// </summary>
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
        public Spi.Models.ModelsBase SquashedEntity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any <see cref="EntityAdapterException" />s that
        /// were thrown during the initial calling of entity adapters.
        /// </summary>
        public IEnumerable<EntityAdapterException> EntityAdapterExceptions
        {
            get;
            set;
        }
    }
}