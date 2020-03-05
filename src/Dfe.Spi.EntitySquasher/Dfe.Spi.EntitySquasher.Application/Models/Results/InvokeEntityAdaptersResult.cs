namespace Dfe.Spi.EntitySquasher.Application.Models.Result
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;

    /// <summary>
    /// Result object for
    /// <see cref="IEntityAdapterInvoker.InvokeEntityAdaptersAsync(string, string, IEnumerable{string}, AggregatesRequest, EntityReference, CancellationToken)" />.
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