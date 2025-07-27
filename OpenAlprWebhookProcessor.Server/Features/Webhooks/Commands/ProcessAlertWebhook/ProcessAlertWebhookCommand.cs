using MediatR;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessAlertWebhook
{
    public class ProcessAlertWebhookCommand : IRequest
    {
        public Webhook Webhook { get; set; }
        public bool IsBulkImport { get; set; }

        public ProcessAlertWebhookCommand(Webhook webhook, bool isBulkImport = false)
        {
            Webhook = webhook;
            IsBulkImport = isBulkImport;
        }
    }
} 