using MediatR;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertWebPush
{
    public class UpsertWebPushCommandHandler : IRequestHandler<UpsertWebPushCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UpsertWebPushClientRequestHandler _upsertWebPushClientRequestHandler;

        public UpsertWebPushCommandHandler(
            IUnitOfWork unitOfWork,
            UpsertWebPushClientRequestHandler upsertWebPushClientRequestHandler)
        {
            _unitOfWork = unitOfWork;
            _upsertWebPushClientRequestHandler = upsertWebPushClientRequestHandler;
        }

        public async Task Handle(UpsertWebPushCommand request, CancellationToken cancellationToken)
        {
            await _upsertWebPushClientRequestHandler.HandleAsync(request.Request);
        }
    }
} 