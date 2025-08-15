using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public interface IGroupWebhookHandler
    {
        Task HandleWebhookAsync(
            Webhook webhook,
            bool isBulkImport,
            CancellationToken cancellationToken = default);
    }
} 