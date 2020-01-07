namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Application.Definitions;

    /// <summary>
    /// Result object for
    /// <see cref="IEntityAdapterInvoker.InvokeEntityAdapters(string, string, IEnumerable{string}, EntityReference)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvokeEntityAdaptersResult : EntityAdapterResult
    {
        /// <summary>
        /// Gets or sets the looked up <see cref="Spi.Models.ModelsBase" />
        /// instances.
        /// </summary>
        public IEnumerable<Spi.Models.ModelsBase> ModelsBases
        {
            get;
            set;
        }
    }
}