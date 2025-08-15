using Mediator;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessPlateGroupWebhook
{
    public class ProcessPlateGroupWebhookCommandHandler : ICommandHandler<ProcessPlateGroupWebhookCommand>
    {
        private readonly IGroupWebhookHandler _groupWebhookHandler;

        public ProcessPlateGroupWebhookCommandHandler(
            IGroupWebhookHandler groupWebhookHandler)
        {
            _groupWebhookHandler = groupWebhookHandler;
        }

        public async ValueTask<Unit> Handle(ProcessPlateGroupWebhookCommand request, CancellationToken cancellationToken = default)
        {
            await _groupWebhookHandler.HandleWebhookAsync(
                request.Webhook,
                request.IsBulkImport,
                cancellationToken);
            return Unit.Value;
        }
    }
} 