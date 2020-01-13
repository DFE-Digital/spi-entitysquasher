namespace Dfe.Spi.EntitySquasher.Domain.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Contains error detail, to be returned to the caller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntityAdapterErrorDetail
    {
        /// <summary>
        /// Gets or sets the originating adapter name.
        /// </summary>
        public string AdapterName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the originally requested entity.
        /// </summary>
        public string RequestedEntityName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the originally requested entity id.
        /// </summary>
        public string RequestedId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the originally requested list of fields.
        /// </summary>
        public IEnumerable<string> RequestedFields
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the returned <see cref="System.Net.HttpStatusCode" />.
        /// </summary>
        public HttpStatusCode HttpStatusCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="Common.Models.HttpErrorBody" /> detail,
        /// if available.
        /// </summary>
        public HttpErrorBody HttpErrorBody
        {
            get;
            set;
        }
    }
}