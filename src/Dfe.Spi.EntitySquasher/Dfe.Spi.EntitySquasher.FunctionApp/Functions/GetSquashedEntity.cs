namespace Dfe.Spi.EntitySquasher.FunctionApp.Functions
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// Entry class for the <c>get-squashed-entity</c> function.
    /// </summary>
    public class GetSquashedEntity
    {
        private readonly IGetSquashedEntityProcessor getSquashedEntityProcessor;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="GetSquashedEntity" />
        /// class.
        /// </summary>
        /// <param name="getSquashedEntityProcessor">
        /// An instance of type <see cref="IGetSquashedEntityProcessor" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GetSquashedEntity(
            IGetSquashedEntityProcessor getSquashedEntityProcessor,
            ILoggerWrapper loggerWrapper)
        {
            this.getSquashedEntityProcessor = getSquashedEntityProcessor;
            this.loggerWrapper = loggerWrapper;
        }

        /// <summary>
        /// Entry method for the <c>get-squashed-entity</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpContext" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName("get-squashed-entity")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = null)]
            HttpRequest httpRequest)
        {
            IActionResult toReturn = null;

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            // Set the context of the logger.
            this.loggerWrapper.SetContext(httpRequest.Headers);

            GetSquashedEntityRequest getSquashedEntityRequest = null;
            try
            {
                getSquashedEntityRequest = this.ParseRequest(httpRequest);
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.loggerWrapper.Warning(
                    $"A {nameof(JsonReaderException)} was thrown during the " +
                    $"parsing of the request.",
                    jsonReaderException);
            }

            if (getSquashedEntityRequest != null)
            {
                this.loggerWrapper.Debug(
                    $"Invoking {nameof(IGetSquashedEntityProcessor)}...");

                toReturn = await this.ProcessWellFormedRequestAsync(
                    getSquashedEntityRequest)
                    .ConfigureAwait(false);
            }
            else
            {
                int statusCode = StatusCodes.Status400BadRequest;

                this.loggerWrapper.Warning(
                    $"The {nameof(HttpRequest)} either had no body, or the " +
                    $"body was not well-formed JSON. " +
                    $"{nameof(statusCode)} {statusCode} will be returned.");

                // No body supplied - return 400 to reflect this.
                toReturn = new StatusCodeResult(statusCode);
            }

            return toReturn;
        }

        private async Task<IActionResult> ProcessWellFormedRequestAsync(
            GetSquashedEntityRequest getSquashedEntityRequest)
        {
            IActionResult toReturn = null;

            try
            {
                GetSquashedEntityResponse getSquashedEntityResponse =
                    await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                        getSquashedEntityRequest)
                        .ConfigureAwait(false);

                this.loggerWrapper.Info(
                    $"{nameof(IGetSquashedEntityProcessor)} invoked with " +
                    $"success.");

                ModelsBase modelsBase = getSquashedEntityResponse.ModelsBase;

                this.loggerWrapper.Info(
                    $"Returning {nameof(modelsBase)}: {modelsBase}.");

                toReturn = new JsonResult(modelsBase);
            }
            catch (FileNotFoundException)
            {
                int statusCode = StatusCodes.Status404NotFound;

                this.loggerWrapper.Warning(
                    $"The processor threw a " +
                    $"{nameof(FileNotFoundException)}. {nameof(statusCode)} " +
                    $"{statusCode} will be returned.");

                // An ACDF could not be found for the specified algorithm.
                // Return 404 to reflect this.
                toReturn = new StatusCodeResult(statusCode);
            }

            return toReturn;
        }

        private GetSquashedEntityRequest ParseRequest(HttpRequest httpRequest)
        {
            GetSquashedEntityRequest toReturn = null;

            string getSquashedEntityRequestStr = null;
            using (StreamReader streamReader = new StreamReader(httpRequest.Body))
            {
                getSquashedEntityRequestStr = streamReader.ReadToEnd();
            }

            this.loggerWrapper.Debug(
                $"Body of request read, as a string value: " +
                $"\"{getSquashedEntityRequestStr}\". Deserialising into a " +
                $"{nameof(GetSquashedEntityRequest)} instance...");

            toReturn =
                JsonConvert.DeserializeObject<GetSquashedEntityRequest>(
                    getSquashedEntityRequestStr);

            return toReturn;
        }
    }
}