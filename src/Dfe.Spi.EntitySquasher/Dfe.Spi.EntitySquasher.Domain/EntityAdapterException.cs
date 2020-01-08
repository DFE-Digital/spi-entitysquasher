namespace Dfe.Spi.EntitySquasher.Domain
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using Dfe.Spi.Common.Http.Client;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Represents an error raised upon calling a an entity adapter web
    /// service, and it returning a non-successful status code, and optionally,
    /// a <see cref="HttpErrorBody" /> instance.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Additional overloads, except for the paramterless one for serialisation, not required, as exception will not be inherited from.")]
    [ExcludeFromCodeCoverage]
    public class EntityAdapterException : SpiWebServiceException
    {
        private new const string Message =
            "The entity adapter \"{0}\" returned a non-successful status " +
            "code, {1}, whilst requesting entity id \"{2}\" and {3} " +
            "field(s). Inspect the properties of this exception for further " +
            "detail.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterException" /> class.
        /// </summary>
        public EntityAdapterException()
        {
            // Nothing - simply used for serialisation purposes.
        }

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterException" /> class.
        /// </summary>
        /// <param name="adapterName">
        /// The originating adapter name.
        /// </param>
        /// <param name="requestedEntityName">
        /// The name of the originally requested entity.
        /// </param>
        /// <param name="requestedId">
        /// The originally requested entity id.
        /// </param>
        /// <param name="requestedFields">
        /// The originally requested list of fields.
        /// </param>
        /// <param name="httpStatusCode">
        /// The actual <see cref="HttpStatusCode" /> resulting in the exception
        /// being thrown.
        /// </param>
        /// <param name="httpErrorBody">
        /// An instance of type <see cref="HttpErrorBody" />.
        /// </param>
        public EntityAdapterException(
            string adapterName,
            string requestedEntityName,
            string requestedId,
            IEnumerable<string> requestedFields,
            HttpStatusCode httpStatusCode,
            HttpErrorBody httpErrorBody)
            : base(
                  BuildExceptionMessage(
                    adapterName,
                    httpStatusCode,
                    requestedId,
                    requestedFields))
        {
            this.AdapterName = adapterName;
            this.RequestedEntityName = requestedEntityName;
            this.RequestedId = requestedId;
            this.RequestedFields = requestedFields;
            this.HttpStatusCode = httpStatusCode;
            this.HttpErrorBody = httpErrorBody;
        }

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

        private static string BuildExceptionMessage(
            string adapterName,
            HttpStatusCode httpStatusCode,
            string requestedId,
            IEnumerable<string> requestedFields)
        {
            string toReturn = null;

            int statusCode = (int)httpStatusCode;

            toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                adapterName,
                statusCode,
                requestedId,
                requestedFields.Count());

            return toReturn;
        }
    }
}