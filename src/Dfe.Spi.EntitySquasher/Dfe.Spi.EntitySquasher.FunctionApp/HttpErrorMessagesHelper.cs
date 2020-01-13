namespace Dfe.Spi.EntitySquasher.FunctionApp
{
    using System.Globalization;
    using System.Net;
    using Dfe.Spi.Common.Http.Server;

    /// <summary>
    /// Contains helper methods for the <see cref="HttpErrorMessages" />
    /// class.
    /// </summary>
    public static class HttpErrorMessagesHelper
    {
        private const string ErrorIdentifierFormat = "SPI-ESQ-{0}";
        private const string ResourceIdentifierFormat = "_{0}";

        /// <summary>
        /// Creates an instance of <see cref="HttpErrorBodyResult" />, using
        /// detail stored in the <see cref="HttpErrorMessages" /> resources
        /// file.
        /// </summary>
        /// <param name="httpStatusCode">
        /// A <see cref="HttpStatusCode" /> value.
        /// </param>
        /// <param name="errorIdentifierInt">
        /// The id of the error message resource as stored in
        /// <see cref="HttpErrorMessages" />, minus the underscore.
        /// </param>
        /// <param name="messageArguments">
        /// Any arguments to populate the error message pulled from
        /// <see cref="HttpErrorMessages" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="HttpErrorBodyResult" />.
        /// </returns>
        public static HttpErrorBodyResult GetHttpErrorBodyResult(
            HttpStatusCode httpStatusCode,
            int errorIdentifierInt,
            params string[] messageArguments)
        {
            HttpErrorBodyResult toReturn = null;

            string errorIdentifier = string.Format(
                CultureInfo.InvariantCulture,
                ErrorIdentifierFormat,
                errorIdentifierInt);

            string name = string.Format(
                CultureInfo.InvariantCulture,
                ResourceIdentifierFormat,
                errorIdentifierInt);

            string message = HttpErrorMessages.ResourceManager.GetString(
                name,
                CultureInfo.InvariantCulture);

            message = string.Format(
                CultureInfo.InvariantCulture,
                message,
                messageArguments);

            toReturn = new HttpErrorBodyResult(
                httpStatusCode,
                errorIdentifier,
                message);

            return toReturn;
        }
    }
}