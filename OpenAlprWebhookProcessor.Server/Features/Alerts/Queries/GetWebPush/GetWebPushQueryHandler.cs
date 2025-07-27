using MediatR;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.VapidKeys;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetWebPush
{
    public class GetWebPushQueryHandler : IRequestHandler<GetWebPushQuery, WebPushRequest>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWebPushQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WebPushRequest> Handle(
            GetWebPushQuery request,
            CancellationToken cancellationToken = default)
        {
            var client = await _unitOfWork.WebPushSettings.GetFirstAsync(cancellationToken);

            if (client == null || string.IsNullOrWhiteSpace(client.PublicKey))
            {
                client = await VapidKeyHelper.AddVapidKeysAsync(_unitOfWork, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new WebPushRequest()
            {
                IsEnabled = client.IsEnabled,
                EmailAddress = client.Subject,
                PublicKey = client.PublicKey,
                PrivateKey = client.PrivateKey,
                SendEveryPlateEnabled = client.SendEveryPlateEnabled,
            };
        }
    }
} 