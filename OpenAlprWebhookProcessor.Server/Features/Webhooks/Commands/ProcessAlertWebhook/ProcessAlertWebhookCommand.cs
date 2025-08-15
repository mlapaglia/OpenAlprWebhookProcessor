using Mediator;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessAlertWebhook
{
    public class ProcessAlertWebhookCommand : ICommand
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