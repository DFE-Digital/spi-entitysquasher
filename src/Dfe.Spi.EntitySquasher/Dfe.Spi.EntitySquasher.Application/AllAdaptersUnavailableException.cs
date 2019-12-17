namespace Dfe.Spi.EntitySquasher.Application
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using Dfe.Spi.Common.Http.Client;

    /// <summary>
    /// Represents an error raised upon being unable to serve a request due to
    /// all adapters returning <see cref="SpiWebServiceException" />s.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "This is internally used, and will not be serialised.")]
    public class AllAdaptersUnavailableException : WebException
    {
        private new const string Message =
            "All {0} request(s) sent to the adapters returned " +
            "non-successful status codes. See the SpiWebServiceExceptions " +
            "property for more information.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AllAdaptersUnavailableException" /> class.
        /// </summary>
        /// <param name="spiWebServiceExceptions">
        /// A set of <see cref="SpiWebServiceException" /> instances.
        /// </param>
        public AllAdaptersUnavailableException(
            IEnumerable<SpiWebServiceException> spiWebServiceExceptions)
            : base(BuildExceptionMessage(spiWebServiceExceptions))
        {
            this.SpiWebServiceExceptions = spiWebServiceExceptions;
        }

        /// <summary>
        /// Gets a set of <see cref="SpiWebServiceException" /> instances.
        /// </summary>
        public IEnumerable<SpiWebServiceException> SpiWebServiceExceptions
        {
            get;
            private set;
        }

        private static string BuildExceptionMessage(
            IEnumerable<SpiWebServiceException> spiWebServiceExceptions)
        {
            string toReturn = null;

            int count = spiWebServiceExceptions.Count();

            toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                count);

            return toReturn;
        }
    }
}