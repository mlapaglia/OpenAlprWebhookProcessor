using Mediator;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessSinglePlateWebhook
{
    public class ProcessSinglePlateWebhookCommandHandler : ICommandHandler<ProcessSinglePlateWebhookCommand>
    {
        private readonly SinglePlateWebhookHandler _singlePlateWebhookHandler;

        public ProcessSinglePlateWebhookCommandHandler(
            SinglePlateWebhookHandler singlePlateWebhookHandler)
        {
            _singlePlateWebhookHandler = singlePlateWebhookHandler;
        }

        public async ValueTask<Unit> Handle(ProcessSinglePlateWebhookCommand request, CancellationToken cancellationToken)
        {
            await _singlePlateWebhookHandler.HandleWebhookAsync(
                request.SinglePlate,
                cancellationToken);
            return Unit.Value;
        }
    }
} 