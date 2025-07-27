using MediatR;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertWebPush
{
    public class UpsertWebPushCommandHandler : IRequestHandler<UpsertWebPushCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertWebPushCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpsertWebPushCommand request, CancellationToken cancellationToken = default)
        {
            var webPushClient = await _unitOfWork.WebPushSettings.GetFirstAsync(cancellationToken);

            if (webPushClient == null)
            {
                webPushClient = new WebPushSettings();
                await _unitOfWork.WebPushSettings.AddAsync(webPushClient, cancellationToken);
            }

            webPushClient.IsEnabled = request.Request.IsEnabled;
            webPushClient.SendEveryPlateEnabled = request.Request.SendEveryPlateEnabled;
            webPushClient.Subject = request.Request.EmailAddress;

            _unitOfWork.WebPushSettings.Update(webPushClient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
} 