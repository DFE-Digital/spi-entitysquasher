namespace Dfe.Spi.EntitySquasher.Domain.Models.Acdf
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Abstract base class for all named settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class NamedSettingEntityBase : AcdfBase
    {
        /// <summary>
        /// Gets or sets the name of the setting entry.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
    }
}