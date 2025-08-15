using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public interface IWebhookForwarder
    {
        Task ForwardWebhookAsync(
            object webhook,
            Uri forwardUrl,
            bool ignoreSslErrors,
            CancellationToken cancellationToken = default);
    }
}