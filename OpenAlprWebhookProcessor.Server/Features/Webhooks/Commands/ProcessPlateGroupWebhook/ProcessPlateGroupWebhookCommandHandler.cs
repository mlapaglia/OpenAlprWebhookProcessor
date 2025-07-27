using MediatR;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessPlateGroupWebhook
{
    public class ProcessPlateGroupWebhookCommandHandler : IRequestHandler<ProcessPlateGroupWebhookCommand>
    {
        private readonly IGroupWebhookHandler _groupWebhookHandler;

        public ProcessPlateGroupWebhookCommandHandler(
            IGroupWebhookHandler groupWebhookHandler)
        {
            _groupWebhookHandler = groupWebhookHandler;
        }

        public async Task Handle(ProcessPlateGroupWebhookCommand request, CancellationToken cancellationToken = default)
        {
            await _groupWebhookHandler.HandleWebhookAsync(
                request.Webhook,
                request.IsBulkImport,
                cancellationToken);
        }
    }
} 