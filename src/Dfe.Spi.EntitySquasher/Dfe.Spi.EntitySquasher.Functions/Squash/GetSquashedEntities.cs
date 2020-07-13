using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Http.Server;
using Dfe.Spi.Common.Http.Server.Definitions;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.Models;
using Dfe.Spi.EntitySquasher.Application.Squash;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Dfe.Spi.EntitySquasher.Functions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Dfe.Spi.Registry.Functions.Squash
{
    public class GetSquashedEntities : FunctionsBase<SquashRequest>
    {
        private readonly ISquashManager _squashManager;

        public GetSquashedEntities(
            ISquashManager squashManager,
            IHttpSpiExecutionContextManager httpSpiExecutionContextManager, 
            ILoggerWrapper loggerWrapper) 
            : base(httpSpiExecutionContextManager, loggerWrapper)
        {
            _squashManager = squashManager;
        }
        
        [FunctionName("GetSquashedEntities")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "get-squashed-entity")]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            return await ValidateAndRunAsync(req, null, cancellationToken);
        }

        protected override HttpErrorBodyResult GetMalformedErrorResponse(FunctionRunContext runContext)
        {
            return new HttpErrorBodyResult(
                new HttpErrorBody
                {
                    Message = "The supplied body was either empty, or not well-formed JSON.",
                    ErrorIdentifier = "SPI-ESQ-1",
                    StatusCode = HttpStatusCode.BadRequest,
                });
        }

        protected override HttpErrorBodyResult GetSchemaValidationResponse(JsonSchemaValidationException validationException, FunctionRunContext runContext)
        {
            return new HttpSchemaValidationErrorBodyResult(
                "SPI-ESQ-2",
                validationException);
        }

        protected override async Task<IActionResult> ProcessWellFormedRequestAsync(
            SquashRequest request, 
            FunctionRunContext runContext, 
            CancellationToken cancellationToken)
        {
            var response = await _squashManager.SquashAsync(request, cancellationToken);
            var statusCode = HttpStatusCode.OK;
            
            // TODO: Process response for non-200 status codes
            
            return new FormattedJsonResult(response, statusCode);
        }
    }
}