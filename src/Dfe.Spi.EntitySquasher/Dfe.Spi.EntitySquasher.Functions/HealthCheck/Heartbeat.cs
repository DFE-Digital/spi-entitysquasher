using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Dfe.Spi.EntitySquasher.Functions.HealthCheck
{
    public class Heartbeat
    {
        [FunctionName("Heartbeat")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            return new OkResult();
        }
    }
}