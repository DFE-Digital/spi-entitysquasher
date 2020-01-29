namespace Dfe.Spi.EntitySquasher.FunctionApp.Functions
{
    using System;
    using Dfe.Spi.Common.Logging.Definitions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// Entry class for the <c>HeartBeat</c> function.
    /// Note: EAPIM's health check is *not* looking for kebab-case.
    ///       So, this is a one-off.
    /// </summary>
    public class HeartBeat
    {
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="HeartBeat" />
        /// class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public HeartBeat(ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }

        /// <summary>
        /// Entry method for the <c>HeartBeat</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpRequest" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName(nameof(HeartBeat))]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequest httpRequest)
        {
            OkResult toReturn = null;

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            IHeaderDictionary headers = httpRequest.Headers;

            this.loggerWrapper.SetContext(headers);

            this.loggerWrapper.Debug("Heart-beat function invoked.");

            toReturn = new OkResult();

            return toReturn;
        }
    }
}