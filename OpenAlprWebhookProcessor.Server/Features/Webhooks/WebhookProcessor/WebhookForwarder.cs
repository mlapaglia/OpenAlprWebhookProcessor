using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public class WebhookForwarder : IWebhookForwarder
    {
        private readonly IFlurlClientCache _flurlClientCache;

        public WebhookForwarder(IFlurlClientCache flurlClientCache)
        {
            _flurlClientCache = flurlClientCache;
        }

        public async Task ForwardWebhookAsync(
            object webhook,
            Uri forwardUrl,
            bool ignoreSslErrors,
            CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient(
                forwardUrl,
                ignoreSslErrors);

            var serializedWebhook = JsonSerializer.Serialize(webhook);

            await client
                .Request(forwardUrl.PathAndQuery)
                .PostStringAsync(
                serializedWebhook,
                cancellationToken: cancellationToken);
        }

        private IFlurlClient GetConfiguredFlurlClient(
            Uri forwardUrl,
            bool ignoreSslErrors)
        {
            var baseUrl = $"{forwardUrl.Scheme}://{forwardUrl.Authority}";
            
            return _flurlClientCache.GetOrAdd($"webhook*{baseUrl}", baseUrl, (fluentClientBuilder) =>
            {
                if (ignoreSslErrors)
                {
                    fluentClientBuilder.ConfigureInnerHandler(handler =>
                    {
                        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    });
                }
            });
        }
    }
}
