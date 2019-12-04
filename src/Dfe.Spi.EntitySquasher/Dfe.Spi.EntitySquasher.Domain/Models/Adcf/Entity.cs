namespace Dfe.Spi.EntitySquasher.Domain.Models.Adcf
{
    using System.Collections.Generic;

    /// <summary>
    /// The serialised form of an entity entry.
    /// </summary>
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