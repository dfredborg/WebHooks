using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebHooks
{
    public class TimerReNew
    {
        [FunctionName("RenewSubscription")]
        public async Task Run([TimerTrigger("0 30 9 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            BusinessCentralService bcService = new BusinessCentralService();
            await bcService.InitializeAsync();
            log.LogInformation($"bcService initialized.");
            var bcResponse = await bcService.GetBusinessCentralAPI("/api/v1.0/subscriptions");
            log.LogInformation($"bcResponse: {bcResponse}");
            var responseObj = JsonConvert.DeserializeObject<SubscriptionResponse>(bcResponse);
            if (responseObj?.Value != null && responseObj.Value.Any())
            {
                var matchingSubscription = responseObj.Value.FirstOrDefault(sub => sub.NotificationUrl == "https://webhooks20230425143351.azurewebsites.net/api/MyItemNotification?code=xxxx");
                if (matchingSubscription != null)
                {
                    await bcService.PatchBusinessCentralAPI(matchingSubscription.SubscriptionId);
                }
                else
                {
                    log.LogInformation("No matching subscription found.");
                }
            }
            else
            {
                log.LogInformation("No subscriptions found in bcResponse.");
            }
        }

        public class SubscriptionResponse
        {
            [JsonProperty("@odata.context")]
            public string ODataContext { get; set; }

            public List<Subscription> Value { get; set; }
        }

        public class Subscription
        {
            [JsonProperty("subscriptionId")]
            public string SubscriptionId { get; set; }

            [JsonProperty("notificationUrl")]
            public string NotificationUrl { get; set; }

            // Include other relevant properties as needed
        }
    }
}
