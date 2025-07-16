using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessSinglePlateWebhook
{
    public class ProcessSinglePlateWebhookCommandHandler : IRequestHandler<ProcessSinglePlateWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SinglePlateWebhookHandler _singlePlateWebhookHandler;

        public ProcessSinglePlateWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            SinglePlateWebhookHandler singlePlateWebhookHandler)
        {
            _unitOfWork = unitOfWork;
            _singlePlateWebhookHandler = singlePlateWebhookHandler;
        }

        public async Task Handle(ProcessSinglePlateWebhookCommand request, CancellationToken cancellationToken)
        {
            await _singlePlateWebhookHandler.HandleWebhookAsync(
                request.SinglePlate,
                cancellationToken);
        }
    }
} 