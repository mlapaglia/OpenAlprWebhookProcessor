using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public interface IGroupWebhookHandler
    {
        Task HandleWebhookAsync(
            Webhook webhook,
            bool isBulkImport,
            CancellationToken cancellationToken);
    }
} 