namespace Dfe.Spi.EntitySquasher.Domain.Models.Acdf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The serialised form of an entity adapter entry.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntityAdapter : NamedSettingEntityBase
    {
        /// <summary>
        /// Gets or sets the base URL of the entity adapter.
        /// </summary>
        public Uri BaseUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any custom headers to be supplied with the call to the
        /// entity adapter.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Usage",
            "CA2227",
            Justification = "This property will be deserialised as a collection of string key-value pairs.")]
        public Dictionary<string, string> Headers
        {
            get;
            set;
        }
    }
}