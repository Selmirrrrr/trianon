using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Trianon.CAM.JTIEmail
{
    public class NewEmail
    {
        private readonly ILogger<NewEmail> _logger;

        public NewEmail(ILogger<NewEmail> logger)
        {
            _logger = logger;
        }

        [Function("NewEmail")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
