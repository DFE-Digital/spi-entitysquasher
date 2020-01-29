namespace Dfe.Spi.EntitySquasher.FunctionApp.Functions
{
    using Dfe.Spi.Common.Logging.Definitions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// Entry class for the <c>heart-beat</c> function.
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
        /// Entry method for the <c>heart-beat</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpRequest" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName("heart-beat")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequest httpRequest)
        {
            OkResult toReturn = null;

            this.loggerWrapper.Debug("Heart-beat function invoked.");

            toReturn = new OkResult();

            return toReturn;
        }
    }
}