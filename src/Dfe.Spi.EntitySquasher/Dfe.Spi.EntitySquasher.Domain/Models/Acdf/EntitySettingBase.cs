namespace Dfe.Spi.EntitySquasher.Domain.Models.Acdf
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Abstract base class for all entity settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
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