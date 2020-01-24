namespace Dfe.Spi.EntitySquasher.FunctionApp.Functions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// Entry class for the <c>get-squashed-entity</c> function.
    /// </summary>
    public class GetSquashedEntity : FunctionsBase<GetSquashedEntityRequest>
    {
        private readonly IGetSquashedEntityProcessor getSquashedEntityProcessor;
        private readonly IHttpErrorBodyResultProvider httpErrorBodyResultProvider;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="GetSquashedEntity" />
        /// class.
        /// </summary>
        /// <param name="getSquashedEntityProcessor">
        /// An instance of type <see cref="IGetSquashedEntityProcessor" />.
        /// </param>
        /// <param name="httpErrorBodyResultProvider">
        /// An instance of type <see cref="IHttpErrorBodyResultProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GetSquashedEntity(
            IGetSquashedEntityProcessor getSquashedEntityProcessor,
            IHttpErrorBodyResultProvider httpErrorBodyResultProvider,
            ILoggerWrapper loggerWrapper)
            : base(loggerWrapper)
        {
            this.getSquashedEntityProcessor = getSquashedEntityProcessor;
            this.httpErrorBodyResultProvider = httpErrorBodyResultProvider;
            this.loggerWrapper = loggerWrapper;
        }

        /// <summary>
        /// Entry method for the <c>get-squashed-entity</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpContext" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName("get-squashed-entity")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = null)]
            HttpRequest httpRequest,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = await this.ValidateAndRunAsync(
                httpRequest,
                cancellationToken)
                .ConfigureAwait(false);

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetMalformedErrorResponse()
        {
            HttpErrorBodyResult toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    1);

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetSchemaValidationResponse(
            string message)
        {
            HttpErrorBodyResult toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    2,
                    message);

            return toReturn;
        }

        /// <inheritdoc />
        protected async override Task<IActionResult> ProcessWellFormedRequestAsync(
            GetSquashedEntityRequest getSquashedEntityRequest,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = null;

            if (getSquashedEntityRequest == null)
            {
                throw new ArgumentNullException(
                    nameof(getSquashedEntityRequest));
            }

            try
            {
                this.loggerWrapper.Debug(
                    $"Invoking {nameof(IGetSquashedEntityProcessor)}...");

                GetSquashedEntityResponse getSquashedEntityResponse =
                    await this.getSquashedEntityProcessor.GetSquashedEntityAsync(
                        getSquashedEntityRequest,
                        cancellationToken)
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

                        toReturn = this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                            HttpStatusCode.FailedDependency,
                            4);
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                this.loggerWrapper.Warning(
                    $"The processor threw a {nameof(FileNotFoundException)}.",
                    fileNotFoundException);

                string algorithm = getSquashedEntityRequest.Algorithm;

                // An ACDF could not be found for the specified algorithm.
                // Return 404 to reflect this.
                toReturn = this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.NotFound,
                    3,
                    algorithm);
            }
            catch (InvalidAlgorithmConfigurationDeclarationFileException invalidAlgorithmConfigurationDeclarationFileException)
            {
                this.loggerWrapper.Error(
                    $"The processor threw a " +
                    $"{nameof(InvalidAlgorithmConfigurationDeclarationFileException)}!",
                    invalidAlgorithmConfigurationDeclarationFileException);

                string message =
                    invalidAlgorithmConfigurationDeclarationFileException.Message;

                toReturn = this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.InternalServerError,
                    5,
                    message);
            }

            return toReturn;
        }
    }
}