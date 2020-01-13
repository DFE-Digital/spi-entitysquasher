namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a reference to an adapter record.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AdapterRecordReference : ModelsBase
    {
        /// <summary>
        /// Gets or sets the id of the record.
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the source (i.e. the
        /// adapter).
        /// </summary>
        public string Source
        {
            get;
            set;
        }
    }
}