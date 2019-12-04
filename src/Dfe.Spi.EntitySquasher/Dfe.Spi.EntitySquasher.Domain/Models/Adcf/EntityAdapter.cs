namespace Dfe.Spi.EntitySquasher.Domain.Models.Adcf
{
    using System;

    /// <summary>
    /// The serialised form of an entity adapter entry.
    /// </summary>
    public class EntityAdapter : NamedSettingEntityBase
    {
        /// <summary>
        /// Gets or sets the URL of the entity adapter.
        /// </summary>
        public Uri Url
        {
            get;
            set;
        }
    }
}