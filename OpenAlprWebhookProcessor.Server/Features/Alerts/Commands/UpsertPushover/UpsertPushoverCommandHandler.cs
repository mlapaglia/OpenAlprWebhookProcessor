using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertPushover
{
    public class UpsertPushoverCommandHandler : ICommandHandler<UpsertPushoverCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertPushoverCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(UpsertPushoverCommand request, CancellationToken cancellationToken = default)
        {
            var pushoverClient = await _unitOfWork.PushoverAlertClients.GetFirstAsync(cancellationToken);

            if (pushoverClient == null)
            {
                pushoverClient = new Data.Pushover();
                await _unitOfWork.PushoverAlertClients.AddAsync(pushoverClient, cancellationToken);
            }

            pushoverClient.ApiToken = request.Request.ApiToken;
            pushoverClient.UserKey = request.Request.UserKey;
            pushoverClient.IsEnabled = request.Request.IsEnabled;
            pushoverClient.SendEveryPlateEnabled = request.Request.SendEveryPlateEnabled;
            pushoverClient.SendPlatePreview = request.Request.SendPlatePreviewEnabled;

            _unitOfWork.PushoverAlertClients.Update(pushoverClient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
} 