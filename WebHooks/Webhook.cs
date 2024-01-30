using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebHooks
{
    public static class Webhook
    {
        [FunctionName("MyItemNotification")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string validationToken = req.Query["validationToken"];
            if (!String.IsNullOrWhiteSpace(validationToken))
            {
                log.LogInformation($"ValidationToken: {validationToken}");
                return new OkObjectResult(validationToken);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            log.LogInformation("New notification:");
            log.LogInformation(requestBody);

            BusinessCentralService bcService = new BusinessCentralService();
            await bcService.InitializeAsync();
            log.LogInformation($"bcService initialized.");
            var bcResponse = await bcService.ProccesNotificationBusinessCentralAPI(data.resource.value.ToString());
            log.LogInformation($"bcResponse: {bcResponse}");

            return new AcceptedResult();
        }
    }
}
