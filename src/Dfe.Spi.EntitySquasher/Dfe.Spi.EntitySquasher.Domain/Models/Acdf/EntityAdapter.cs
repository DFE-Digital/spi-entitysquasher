﻿namespace Dfe.Spi.EntitySquasher.Domain.Models.Acdf
{
    using System;
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
    }
}