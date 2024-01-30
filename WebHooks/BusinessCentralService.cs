using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebHooks
{
    public class BusinessCentralService
    {
        private string AadTenantId { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string BCCompanyId { get; set; }
        private string BCEnvironmentName { get; set; }
        private string AccessToken { get; set; }

        private static readonly HttpClient client = new HttpClient();

        public BusinessCentralService()
        {
            this.AadTenantId = Environment.GetEnvironmentVariable("BcAadTenantId");
            this.ClientId = Environment.GetEnvironmentVariable("BcClientId");
            this.ClientSecret = Environment.GetEnvironmentVariable("BcClientSecret");
            this.BCCompanyId = Environment.GetEnvironmentVariable("BcCompanyID");
            this.BCEnvironmentName = Environment.GetEnvironmentVariable("BCEnvironmentName");

        }

        public async Task InitializeAsync()
        {
            await GetAccessToken();
        }

        public async Task<string> GetAccessToken()
        {
            try
            {

                string authority = $"https://login.microsoftonline.com/{AadTenantId}";
                string[] scopes = new string[] { "https://api.businesscentral.dynamics.com/.default" };

                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                    .Create(ClientId)
                    .WithClientSecret(ClientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

                AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                AccessToken = result?.AccessToken;
                return AccessToken;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to acquire token: {ex.Message}", ex);
            }
        }

        public async Task<string> PatchBusinessCentralAPI(string subscriptionId)
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                AddOrUpdateHeader(client, "If-Match", "*");


                string url = $"https://api.businesscentral.dynamics.com/v2.0/{AadTenantId}/{BCEnvironmentName}/api/v1.0/subscriptions('{subscriptionId}')";

                HttpContent httpContent = new StringContent("", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, httpContent);

                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Call to Business Central API failed: {response.StatusCode} {response.ReasonPhrase} {responseContent}");
                }

                return responseContent;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to call Business Central API: {ex.Message}", ex);
            }
        }

        public async Task<string> GetBusinessCentralAPI(string input)
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                string url = $"https://api.businesscentral.dynamics.com/v2.0/{AadTenantId}/{BCEnvironmentName}" + input;

                HttpResponseMessage response = await client.GetAsync(url);

                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Call to Business Central API failed: {response.StatusCode} {response.ReasonPhrase} {responseContent}");
                }

                return responseContent;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to call Business Central API: {ex.Message}", ex);
            }
        }

        public async Task<string> ProccesNotificationBusinessCentralAPI(string input)
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(input);

                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Call to Business Central API failed: {response.StatusCode} {response.ReasonPhrase} {responseContent}");
                }

                return responseContent;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to call Business Central API: {ex.Message}", ex);
            }
        }

        private void AddOrUpdateHeader(HttpClient client, string headerName, string headerValue)
        {
            if (client.DefaultRequestHeaders.Contains(headerName))
            {
                client.DefaultRequestHeaders.Remove(headerName);
            }
            client.DefaultRequestHeaders.Add(headerName, headerValue);
        }
    }
}
