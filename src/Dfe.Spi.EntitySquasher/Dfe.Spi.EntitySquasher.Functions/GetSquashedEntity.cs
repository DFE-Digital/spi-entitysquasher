namespace Dfe.Spi.EntitySquasher.Functions
{
    using System;
    using System.IO;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
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
        private readonly IGetSquashedEntityProcessor getSquashedEntityProcessor;

        /// <summary>
        /// Initialises a new instance of the <see cref="GetSquashedEntity" />
        /// class.
        /// </summary>
        /// <param name="getSquashedEntityProcessor">
        /// An instance of type <see cref="IGetSquashedEntityProcessor" />.
        /// </param>
        public GetSquashedEntity(
            IGetSquashedEntityProcessor getSquashedEntityProcessor)
        {
            this.getSquashedEntityProcessor = getSquashedEntityProcessor;
        }

        /// <summary>
        /// Entry method for the <c>get-squashed-entity</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpContext" />.
        /// </param>
        /// <param name="logger">
        /// An instance of type <see cref="ILogger" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName("get-squashed-entity")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = null)]
            HttpRequest httpRequest,
            ILogger logger)
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

            logger.LogDebug($"Body: {getSquashedEntityRequestStr}");

            GetSquashedEntityRequest getSquashedEntityRequest =
                JsonConvert.DeserializeObject<GetSquashedEntityRequest>(
                    getSquashedEntityRequestStr);

            GetSquashedEntityResponse getSquashedEntityResponse =
                this.getSquashedEntityProcessor.GetSquashedEntity(
                    getSquashedEntityRequest);

            ModelsBase modelsBase = getSquashedEntityResponse.ModelsBase;

            toReturn = new JsonResult(modelsBase);

            return toReturn;
        }
    }
}