namespace Dfe.Spi.EntitySquasher.Domain.Models.Acdf
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The serialised form of a field setting.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Field : EntitySettingBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to treat whitespace
        /// (including empty) <see cref="string" /> values in the same way a
        /// null value would be.
        /// </summary>
        public bool TreatWhitespaceAsNull
        {
            get;
            set;
        }
    }
}