using System.Linq;
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
            try
            {
                var response = await _squashManager.SquashAsync(request, cancellationToken);
                var statusCode = HttpStatusCode.OK;

                if (response.SquashedEntityResults.Count(x => x.SquashedEntity == null) == response.SquashedEntityResults.Length)
                {
                    // No adapter calls have worked
                    return new HttpErrorBodyResult(
                        new HttpErrorBody
                        {
                            Message = "Unable to serve any requests - all adapters are unavailable.",
                            ErrorIdentifier = "SPI-ESQ-4",
                            StatusCode = HttpStatusCode.FailedDependency,
                        });
                }

                if (response.SquashedEntityResults.Count(x => x.EntityAdapterErrorDetails != null && x.EntityAdapterErrorDetails.Any()) > 0)
                {
                    // Some errors, but some worked
                    statusCode = HttpStatusCode.PartialContent;
                }

                return new FormattedJsonResult(response, statusCode);
            }
            catch (ProfileNotFoundException ex)
            {
                return new HttpErrorBodyResult(
                    new HttpErrorBody
                    {
                        Message = $"Could not find an Algorithm Configuration Declaration File for the specified algorithm '{ex.ProfileName}'.",
                        ErrorIdentifier = "SPI-ESQ-3",
                        StatusCode = HttpStatusCode.BadRequest,
                    });
            }
            catch (InvalidRequestException ex)
            {
                return new HttpErrorBodyResult(
                    new HttpErrorBody
                    {
                        Message = ex.Message,
                        ErrorIdentifier = "SPI-ESQ-7",
                        StatusCode = HttpStatusCode.BadRequest,
                    });
            }
        }
    }
}