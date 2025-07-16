using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessAlertWebhook
{
    public class ProcessAlertWebhookCommandHandler : IRequestHandler<ProcessAlertWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GroupWebhookHandler _groupWebhookHandler;

        public ProcessAlertWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            GroupWebhookHandler groupWebhookHandler)
        {
            _unitOfWork = unitOfWork;
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