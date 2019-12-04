namespace Dfe.Spi.EntitySquasher.Domain.Models.Adcf
{
    /// <summary>
    /// Abstract base class for all named settings.
    /// </summary>
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