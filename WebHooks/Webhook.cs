using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var data = JsonConvert.DeserializeObject<NotificationPayload>(requestBody);

            log.LogInformation("New notification:");
            log.LogInformation(requestBody);

            if (data?.Value != null && data.Value.Any())
            {
                BusinessCentralService bcService = new BusinessCentralService();
                await bcService.InitializeAsync();
                log.LogInformation($"bcService initialized.");

                foreach (var item in data.Value)
                {
                    var bcResponse = await bcService.ProccesNotificationBusinessCentralAPI(item.Resource);
                    log.LogInformation($"Processed resource: {item.Resource}");
                    log.LogInformation($"bcResponse: {bcResponse}");
                }
            }
            else
            {
                log.LogInformation("No notification data found.");
            }

            return new AcceptedResult();
        }
    }

    public class NotificationPayload
    {
        public List<SubscriptionInfo> Value { get; set; }
    }

    public class SubscriptionInfo
    {
        public string SubscriptionId { get; set; }
        public string ClientState { get; set; }
        public string ExpirationDateTime { get; set; }
        public string Resource { get; set; }
        public string ChangeType { get; set; }
        public string LastModifiedDateTime { get; set; }
    }
}
