using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor
{
    public interface IGroupWebhookHandler
    {
        Task HandleWebhookAsync(Webhook webhook, bool isBulkImport, CancellationToken cancellationToken);
    }
}