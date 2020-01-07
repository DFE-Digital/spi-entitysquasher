namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Domain;

    /// <summary>
    /// Abstract base class representing results originating from an
    /// entity adapter.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class EntityAdapterResult : ModelsBase
    {
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