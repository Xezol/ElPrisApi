using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace BackgroundWorker.Functions
{
    public static class HealthCheckPingerFunction
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [Function("HealthCheckPinger")]
        public static async Task Run([TimerTrigger("%HealthCheckCron%", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            var discordWebhookUrl = Environment.GetEnvironmentVariable("DiscordWebhookUrl");

            if (string.IsNullOrEmpty(discordWebhookUrl))
            {
                log.LogError("Missing configuration in local.settings.json");
                return;
            }

            var urlsToCheck = Environment.GetEnvironmentVariable("UrlsToCheck").Split(';');

            foreach (var url in urlsToCheck)
            {
                var isUp = await CheckUrlAsync(url);
                if (!isUp)
                {
                    var message = $"The URL {url} is down.";
                    await SendDiscordNotification(discordWebhookUrl, message);
                }

                //If u want to check that it works.
                //else
                //{
                //    var message = $"The URL {url} is up.";
                //    await SendDiscordNotification(discordWebhookUrl, message);
                //}
            }
        }

        private static async Task<bool> CheckUrlAsync(string url)
        {
            try
            {
                var response = await HttpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static async Task SendDiscordNotification(string webhookUrl, string message)
        {
            var payload = new { content = message };
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            await HttpClient.PostAsync(webhookUrl, content);
        }
    }

}
