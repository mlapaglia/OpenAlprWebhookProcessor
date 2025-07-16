using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertPushover
{
    public class UpsertPushoverCommandHandler : IRequestHandler<UpsertPushoverCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertPushoverCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpsertPushoverCommand request, CancellationToken cancellationToken)
        {
            var pushoverClients = await _unitOfWork.PushoverAlertClients.GetAllAsync(cancellationToken);
            var pushoverClient = pushoverClients.FirstOrDefault();

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
        }
    }
} 