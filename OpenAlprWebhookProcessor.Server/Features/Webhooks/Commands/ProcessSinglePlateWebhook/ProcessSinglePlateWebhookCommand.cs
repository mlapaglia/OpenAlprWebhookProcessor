using MediatR;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessSinglePlateWebhook
{
    public class ProcessSinglePlateWebhookCommand : IRequest
    {
        public SinglePlate SinglePlate { get; set; }

        public ProcessSinglePlateWebhookCommand(SinglePlate singlePlate)
        {
            SinglePlate = singlePlate;
        }
    }
} 