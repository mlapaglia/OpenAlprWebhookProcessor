using MediatR;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessSinglePlateWebhook
{
    public class ProcessSinglePlateWebhookCommandHandler : IRequestHandler<ProcessSinglePlateWebhookCommand>
    {
        private readonly SinglePlateWebhookHandler _singlePlateWebhookHandler;

        public ProcessSinglePlateWebhookCommandHandler(
            SinglePlateWebhookHandler singlePlateWebhookHandler)
        {
            _singlePlateWebhookHandler = singlePlateWebhookHandler;
        }

        public async Task Handle(ProcessSinglePlateWebhookCommand request, CancellationToken cancellationToken = default)
        {
            await _singlePlateWebhookHandler.HandleWebhookAsync(
                request.SinglePlate,
                cancellationToken);
        }
    }
} 