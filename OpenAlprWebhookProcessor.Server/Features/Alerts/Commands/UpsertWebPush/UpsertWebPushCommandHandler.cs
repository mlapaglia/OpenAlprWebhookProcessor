using Mediator;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertWebPush
{
    public class UpsertWebPushCommandHandler : ICommandHandler<UpsertWebPushCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertWebPushCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(UpsertWebPushCommand request, CancellationToken cancellationToken)
        {
            var webPushClient = await _unitOfWork.WebPushSettings.GetFirstAsync(cancellationToken);

            if (webPushClient == null)
            {
                webPushClient = new WebPushSettings();
                Map(webPushClient, request.Request);
                await _unitOfWork.WebPushSettings.AddAsync(webPushClient, cancellationToken);
            }
            else
            {
                Map(webPushClient, request.Request);
                _unitOfWork.WebPushSettings.Update(webPushClient);
            }
                
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private static void Map(WebPushSettings webPushClient, WebPushRequest request)
        {
            webPushClient.IsEnabled = request.IsEnabled;
            webPushClient.SendEveryPlateEnabled = request.SendEveryPlateEnabled;
            webPushClient.Subject = request.EmailAddress;
        }
    }
} 