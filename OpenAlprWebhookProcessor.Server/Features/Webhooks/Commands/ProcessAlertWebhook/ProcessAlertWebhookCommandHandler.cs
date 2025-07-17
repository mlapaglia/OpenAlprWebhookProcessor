using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessAlertWebhook
{
    public class ProcessAlertWebhookCommandHandler : IRequestHandler<ProcessAlertWebhookCommand>
    {
        private readonly IGroupWebhookHandler _groupWebhookHandler;

        public ProcessAlertWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            IGroupWebhookHandler groupWebhookHandler)
        {
            _groupWebhookHandler = groupWebhookHandler;
        }

        public async Task Handle(ProcessAlertWebhookCommand request, CancellationToken cancellationToken)
        {
            await _groupWebhookHandler.HandleWebhookAsync(
                request.Webhook,
                request.IsBulkImport,
                cancellationToken);
        }
    }
} 