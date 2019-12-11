namespace Dfe.Spi.EntitySquasher.Domain.Models.Acdf
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The serialised form of an entity entry.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Entity : EntitySettingBase
    {
        /// <summary>
        /// Gets or sets a set of <see cref="Field" /> entities.
        /// </summary>
        public IEnumerable<Field> Fields
        {
            get;
            set;
        }
    }
}