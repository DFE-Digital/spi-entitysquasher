namespace Dfe.Spi.EntitySquasher.Domain.Models.Adcf
{
    using System.Collections.Generic;

    /// <summary>
    /// The serialised form of the algorithm declaration configuration file.
    /// </summary>
    public class AlgorithmDeclarationConfigurationFile : AcdfBase
    {
        /// <summary>
        /// Gets or sets a set of <see cref="Entity" /> instances.
        /// </summary>
        public IEnumerable<Entity> Entities
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a set of <see cref="EntityAdapter" /> instances.
        /// </summary>
        public IEnumerable<EntityAdapter> EntityAdapters
        {
            get;
            set;
        }
    }
}
