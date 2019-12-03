namespace Dfe.Spi.EntitySquasher.FunctionApp.Functions
{
    using System;
    using System.IO;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.Common.Logging.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Entry class for the <c>get-squashed-entity</c> function.
    /// </summary>
    public class GetSquashedEntity
    {
        private readonly IGetSquashedEntityProcessorFactory getSquashedEntityProcessorFactory;
        private readonly ILoggerWrapperFactory loggerWrapperFactory;

        /// <summary>
        /// Initialises a new instance of the <see cref="GetSquashedEntity" />
        /// class.
        /// </summary>
        /// <param name="getSquashedEntityProcessorFactory">
        /// An instance of type
        /// <see cref="IGetSquashedEntityProcessorFactory" />.
        /// </param>
        /// <param name="loggerWrapperFactory">
        /// An instance of type <see cref="ILoggerWrapperFactory" />.
        /// </param>
        public GetSquashedEntity(
            IGetSquashedEntityProcessorFactory getSquashedEntityProcessorFactory,
            ILoggerWrapperFactory loggerWrapperFactory)
        {
            this.getSquashedEntityProcessorFactory = getSquashedEntityProcessorFactory;
            this.loggerWrapperFactory = loggerWrapperFactory;
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
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = null)]
            HttpRequest httpRequest)
        {
            IActionResult toReturn = null;

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            string getSquashedEntityRequestStr = null;
            using (StreamReader streamReader = new StreamReader(httpRequest.Body))
            {
                getSquashedEntityRequestStr = streamReader.ReadToEnd();
            }

            GetSquashedEntityRequest getSquashedEntityRequest =
                JsonConvert.DeserializeObject<GetSquashedEntityRequest>(
                    getSquashedEntityRequestStr);

            ILoggerWrapper loggerWrapper = this.loggerWrapperFactory.Create(
                getSquashedEntityRequest);

            IGetSquashedEntityProcessor getSquashedEntityProcessor =
                this.getSquashedEntityProcessorFactory.Create(
                    loggerWrapper);

            GetSquashedEntityResponse getSquashedEntityResponse =
                getSquashedEntityProcessor.GetSquashedEntity(
                    getSquashedEntityRequest);

            ModelsBase modelsBase = getSquashedEntityResponse.ModelsBase;

            toReturn = new JsonResult(modelsBase);

            return toReturn;
        }
    }
}