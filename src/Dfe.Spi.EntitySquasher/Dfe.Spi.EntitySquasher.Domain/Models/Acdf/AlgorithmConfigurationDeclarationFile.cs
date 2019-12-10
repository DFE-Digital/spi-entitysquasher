﻿namespace Dfe.Spi.EntitySquasher.Domain.Models.Acdf
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The serialised form of the Algorithm Configuration Declaration File.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AlgorithmConfigurationDeclarationFile : AcdfBase
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
