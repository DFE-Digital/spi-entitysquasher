namespace Dfe.Spi.EntitySquasher.Domain
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using Dfe.Spi.Common.Http.Client;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.EntitySquasher.Domain.Models;

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
        /// <param name="entityAdapterErrorDetail">
        /// An instance of <see cref="Models.EntityAdapterErrorDetail" />.
        /// </param>
        /// <param name="httpStatusCode">
        /// The actual <see cref="HttpStatusCode" /> resulting in the exception
        /// being thrown.
        /// </param>
        /// <param name="httpErrorBody">
        /// An instance of type <see cref="HttpErrorBody" />.
        /// </param>
        public EntityAdapterException(
            EntityAdapterErrorDetail entityAdapterErrorDetail,
            HttpStatusCode httpStatusCode,
            HttpErrorBody httpErrorBody)
            : base(
                  BuildExceptionMessage(
                    entityAdapterErrorDetail,
                    httpStatusCode))
        {
            this.EntityAdapterErrorDetail = entityAdapterErrorDetail;
            this.HttpStatusCode = httpStatusCode;
            this.HttpErrorBody = httpErrorBody;
        }

        /// <summary>
        /// Gets an instance of <see cref="Models.EntityAdapterErrorDetail" />.
        /// </summary>
        public EntityAdapterErrorDetail EntityAdapterErrorDetail
        {
            get;
            private set;
        }

        private static string BuildExceptionMessage(
            EntityAdapterErrorDetail entityAdapterErrorDetail,
            HttpStatusCode httpStatusCode)
        {
            string toReturn = null;

            int statusCode = (int)httpStatusCode;

            IEnumerable<string> requestedFields =
                entityAdapterErrorDetail.RequestedFields;

            string fieldsCountStr = null;
            if (requestedFields == null)
            {
                fieldsCountStr = "all";
            }
            else
            {
                int fieldsCount = requestedFields.Count();

                fieldsCountStr = fieldsCount
                    .ToString(CultureInfo.InvariantCulture);
            }

            toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                entityAdapterErrorDetail.AdapterName,
                statusCode,
                entityAdapterErrorDetail.RequestedId,
                fieldsCountStr);

            return toReturn;
        }
    }
}