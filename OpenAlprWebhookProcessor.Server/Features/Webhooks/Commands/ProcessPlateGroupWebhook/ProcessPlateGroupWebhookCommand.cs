using MediatR;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessPlateGroupWebhook
{
    public class ProcessPlateGroupWebhookCommand : IRequest
    {
        public Webhook Webhook { get; set; }
        public bool IsBulkImport { get; set; }

        public ProcessPlateGroupWebhookCommand(Webhook webhook, bool isBulkImport = false)
        {
            Webhook = webhook;
            IsBulkImport = isBulkImport;
        }
    }
} 