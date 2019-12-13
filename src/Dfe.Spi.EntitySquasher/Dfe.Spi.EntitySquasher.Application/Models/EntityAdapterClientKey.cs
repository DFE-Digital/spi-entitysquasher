namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Key type, used by
    /// <see cref="IEntityAdapterClientCache" />.
    /// </summary>
    public class EntityAdapterClientKey : ModelsBase
    {
        /// <summary>
        /// Gets or sets the name of the entity adapter.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the algorithm containing the entity
        /// adapter configuration.
        /// </summary>
        public string Algorithm
        {
            get;
            set;
        }
    }
}