using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using Org.BouncyCastle.Asn1.Ocsp;
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

        public async ValueTask<Unit> Handle(UpsertPushoverCommand request, CancellationToken cancellationToken)
        {
            var pushoverClient = await _unitOfWork.PushoverAlertClients.GetFirstAsync(cancellationToken);

            if (pushoverClient == null)
            {
                pushoverClient = new Data.Pushover();
                Map(pushoverClient, request.Request);
                await _unitOfWork.PushoverAlertClients.AddAsync(pushoverClient, cancellationToken);
            }
            else
            {
                Map(pushoverClient, request.Request);
                _unitOfWork.PushoverAlertClients.Update(pushoverClient);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        private static void Map(
            Data.Pushover pushoverClient,
            PushoverRequest request)
        {
            pushoverClient.ApiToken = request.ApiToken;
            pushoverClient.UserKey = request.UserKey;
            pushoverClient.IsEnabled = request.IsEnabled;
            pushoverClient.SendEveryPlateEnabled = request.SendEveryPlateEnabled;
            pushoverClient.SendPlatePreview = request.SendPlatePreviewEnabled;
        }
    }
} 