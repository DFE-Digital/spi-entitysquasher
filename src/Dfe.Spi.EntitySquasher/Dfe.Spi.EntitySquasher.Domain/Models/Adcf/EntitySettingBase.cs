namespace Dfe.Spi.EntitySquasher.Domain.Models.Adcf
{
    using System.Collections.Generic;

    /// <summary>
    /// Abstract base class for all entity settings.
    /// </summary>
    public abstract class EntitySettingBase : NamedSettingEntityBase
    {
        /// <summary>
        /// Gets or sets a list of sources, in order of preference.
        /// </summary>
        public IEnumerable<string> Sources
        {
            get;
            set;
        }
    }
}