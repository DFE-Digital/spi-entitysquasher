namespace Dfe.Spi.EntitySquasher.FunctionApp.Functions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Newtonsoft.Json;
    using NJsonSchema;
    using NJsonSchema.Validation;

    /// <summary>
    /// Entry class for the <c>get-squashed-entity</c> function.
    /// </summary>
    public class GetSquashedEntity
    {
        private const string SchemaFilename = "get-squashed-entity-body.json";

        private readonly IGetSquashedEntityProcessor getSquashedEntityProcessor;
        private readonly ILoggerWrapper loggerWrapper;

        private JsonSchema jsonSchema;

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
                getSquashedEntityRequest = await this.ParseAndValidateRequest(
                    httpRequest)
                    .ConfigureAwait(false);
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.loggerWrapper.Info(
                    $"A {nameof(JsonReaderException)} was thrown during the " +
                    $"parsing of the body of the request.",
                    jsonReaderException);

                toReturn = HttpErrorMessagesHelper.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    1);
            }
            catch (JsonSchemaValidationException jsonSchemaValidationException)
            {
                this.loggerWrapper.Info(
                    $"A {nameof(JsonSchemaValidationException)} was thrown " +
                    $"during the parsing of the body of the request.",
                    jsonSchemaValidationException);

                string message = jsonSchemaValidationException.Message;

                toReturn = HttpErrorMessagesHelper.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    2,
                    message);
            }

            if (getSquashedEntityRequest != null)
            {
                // The JSON is valid and not null, but at this point, it's
                // unknown if its valid according to the *schema*.
                toReturn = await this.ProcessWellFormedRequestAsync(
                    getSquashedEntityRequest)
                    .ConfigureAwait(false);
            }

            if (toReturn is HttpErrorBodyResult)
            {
                HttpErrorBodyResult httpErrorBodyResult =
                    (HttpErrorBodyResult)toReturn;

                object value = httpErrorBodyResult.Value;

                this.loggerWrapper.Info(
                    $"This HTTP request failed. Returning: {value}.");
            }

            return toReturn;
        }

        private async Task<JsonSchema> LoadJsonSchema()
        {
            JsonSchema toReturn = null;

            if (this.jsonSchema == null)
            {
                this.loggerWrapper.Debug(
                    $"The {nameof(JsonSchema)} for this function hasn't " +
                    $"been loaded yet. Getting the embedded schema as a " +
                    $"string...");

                Type type = typeof(GetSquashedEntity);
                Assembly assembly = type.Assembly;

                string[] embeddedResources =
                    assembly.GetManifestResourceNames();

                string fullPath = embeddedResources
                    .Single(x => x.EndsWith(
                        SchemaFilename,
                        StringComparison.InvariantCulture));

                string dataStr = null;
                using (Stream stream = assembly.GetManifestResourceStream(fullPath))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        dataStr = await streamReader.ReadToEndAsync()
                            .ConfigureAwait(false);
                    }
                }

                this.loggerWrapper.Info(
                    $"{nameof(dataStr)} loaded. Creating " +
                    $"{nameof(JsonSchema)}...");

                // Then load it.
                this.jsonSchema = await JsonSchema.FromJsonAsync(dataStr)
                    .ConfigureAwait(false);
            }

            toReturn = this.jsonSchema;

            this.loggerWrapper.Info(
                $"Returning {nameof(JsonSchema)} stored in memory: " +
                $"{toReturn}.");

            return toReturn;
        }

        private async Task<IActionResult> ProcessWellFormedRequestAsync(
            GetSquashedEntityRequest getSquashedEntityRequest)
        {
            IActionResult toReturn = null;

            try
            {
                this.loggerWrapper.Debug(
                    $"Invoking {nameof(IGetSquashedEntityProcessor)}...");

                GetSquashedEntityResponse getSquashedEntityResponse =
                    await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                        getSquashedEntityRequest)
                        .ConfigureAwait(false);

                this.loggerWrapper.Info(
                    $"{nameof(IGetSquashedEntityProcessor)} invoked with " +
                    $"success.");

                toReturn = new JsonResult(getSquashedEntityResponse);

                // Did one or more errors occur?
                bool adapterErrorHappened = getSquashedEntityResponse
                    .SquashedEntityResults
                    .SelectMany(x => x.EntityAdapterErrorDetails)
                    .Any();

                this.loggerWrapper.Info(
                    $"{nameof(adapterErrorHappened)} = " +
                    $"{adapterErrorHappened}");

                if (adapterErrorHappened)
                {
                    // Do we have *any* results at all?
                    bool resultsExist = getSquashedEntityResponse
                        .SquashedEntityResults
                        .Where(x => x.SquashedEntity != null)
                        .Any();

                    this.loggerWrapper.Info(
                        $"{nameof(resultsExist)} = {resultsExist}");

                    if (resultsExist)
                    {
                        (toReturn as JsonResult).StatusCode =
                            (int)HttpStatusCode.PartialContent;

                        this.loggerWrapper.Info(
                            $"We were able to get some stuff, but there " +
                            $"were some errors. Returning " +
                            $"{HttpStatusCode.PartialContent} with the " +
                            $"results we got.");
                    }
                    else
                    {
                        this.loggerWrapper.Error(
                            "It seems that we were unable to serve ANY " +
                            "requests. Returning an error back to the " +
                            "client.");

                        toReturn = HttpErrorMessagesHelper.GetHttpErrorBodyResult(
                            HttpStatusCode.FailedDependency,
                            4);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                this.loggerWrapper.Warning(
                    $"The processor threw a {nameof(FileNotFoundException)}.");

                // An ACDF could not be found for the specified algorithm.
                // Return 404 to reflect this.
                string algorithm = getSquashedEntityRequest.Algorithm;

                toReturn = HttpErrorMessagesHelper.GetHttpErrorBodyResult(
                    HttpStatusCode.NotFound,
                    3,
                    algorithm);
            }

            return toReturn;
        }

        private async Task<GetSquashedEntityRequest> ParseAndValidateRequest(
            HttpRequest httpRequest)
        {
            GetSquashedEntityRequest toReturn = null;

            // Read the body as a string...
            string getSquashedEntityRequestStr = null;
            using (StreamReader streamReader = new StreamReader(httpRequest.Body))
            {
                getSquashedEntityRequestStr = streamReader.ReadToEnd();
            }

            this.loggerWrapper.Debug(
                $"Body of request read, as a string value: " +
                $"\"{getSquashedEntityRequestStr}\".");

            // Validate against the schema.
            JsonSchema jsonSchema = await this.LoadJsonSchema()
                .ConfigureAwait(false);

            this.loggerWrapper.Debug(
                $"Performing validation of body against " +
                $"{nameof(JsonSchema)}...");

            ICollection<ValidationError> validationErrors =
                jsonSchema.Validate(getSquashedEntityRequestStr);

            if (validationErrors.Count > 0)
            {
                throw new JsonSchemaValidationException(validationErrors);
            }

            this.loggerWrapper.Debug(
                $"Deserialising body into a " +
                $"{nameof(GetSquashedEntityRequest)} instance...");

            toReturn =
                JsonConvert.DeserializeObject<GetSquashedEntityRequest>(
                    getSquashedEntityRequestStr);

            return toReturn;
        }
    }
}