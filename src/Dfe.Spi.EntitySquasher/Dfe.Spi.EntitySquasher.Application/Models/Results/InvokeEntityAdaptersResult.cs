namespace Dfe.Spi.EntitySquasher.Application.Models.Result
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Application.Definitions;

    /// <summary>
    /// Result object for
    /// <see cref="IEntityAdapterInvoker.InvokeEntityAdapters(string, string, IEnumerable{string}, EntityReference)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvokeEntityAdaptersResult : ModelsBase
    {
        /// <summary>
        /// Gets or sets the looked up <see cref="GetEntityAsyncResult" />
        /// instances.
        /// </summary>
        public IEnumerable<GetEntityAsyncResult> GetEntityAsyncResults
        {
            get;
            set;
        }
    }
}