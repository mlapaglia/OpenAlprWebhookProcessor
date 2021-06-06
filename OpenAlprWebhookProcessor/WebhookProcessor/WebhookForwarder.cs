using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public static class WebhookForwarder
    {
        public static async Task ForwardWebhookAsync(
            Webhook webhook,
            Uri forwardUrl,
            bool ignoreSslErrors,
            CancellationToken cancellationToken)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                if (ignoreSslErrors)
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                using (var client = new HttpClient(httpClientHandler))
                {
                    var serializedWebhook = JsonSerializer.Serialize(webhook);
                    var httpContent = new StringContent(serializedWebhook, System.Text.Encoding.UTF8, "application/json");

                    await client.PostAsync(
                        forwardUrl,
                        httpContent,
                        cancellationToken);
                }
            }
        }
    }
}
