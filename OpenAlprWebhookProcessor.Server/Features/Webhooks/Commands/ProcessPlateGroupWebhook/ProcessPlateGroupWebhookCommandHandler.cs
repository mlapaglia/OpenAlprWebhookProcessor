using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessPlateGroupWebhook
{
    public class ProcessPlateGroupWebhookCommandHandler : IRequestHandler<ProcessPlateGroupWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GroupWebhookHandler _groupWebhookHandler;

        public ProcessPlateGroupWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            GroupWebhookHandler groupWebhookHandler)
        {
            _unitOfWork = unitOfWork;
            _groupWebhookHandler = groupWebhookHandler;
        }

        public async Task Handle(ProcessPlateGroupWebhookCommand request, CancellationToken cancellationToken)
        {
            await _groupWebhookHandler.HandleWebhookAsync(
                request.Webhook,
                request.IsBulkImport,
                cancellationToken);
        }
    }
} 