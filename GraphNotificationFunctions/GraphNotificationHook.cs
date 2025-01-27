using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GraphNotificationFunctions
{
    public class GraphNotificationHook
    {
        private readonly ILogger<GraphNotificationHook> _logger;

        public GraphNotificationHook(ILogger<GraphNotificationHook> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GraphNotificationHook))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            const string clientStateSecret = "[the secret that was used when creating the graph event subscription]";

            // parse query parameter
            var validationToken = req.Query["validationToken"];

            //_logger.LogInformation("validationToken: " + validationToken);

            if (!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation("validation token found: " + validationToken);
                _logger.LogInformation("returning 200 response");
                return new ContentResult { Content = validationToken, ContentType = "text/plain" };
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };


            var data = JsonSerializer.Deserialize<GraphNotificationDto>(requestBody, serializeOptions);          

            if (!data.value.FirstOrDefault().ClientState.Equals(clientStateSecret, StringComparison.OrdinalIgnoreCase))
            {
                //client state is not valid (doesn't mach the one submitted with the subscription)
                return new BadRequestResult();
            }

            //do something with the notification data
            _logger.LogInformation("request body: " + requestBody);

            return new OkResult();
        }
    }
}
