using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetPushover
{
    public class GetPushoverQueryHandler : IRequestHandler<GetPushoverQuery, PushoverRequest>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPushoverQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PushoverRequest> Handle(GetPushoverQuery request, CancellationToken cancellationToken)
        {
            var client = await _unitOfWork.PushoverAlertClients.GetFirstAsync(cancellationToken);

            if (client == null)
            {
                return new PushoverRequest();
            }

            return new PushoverRequest()
            {
                ApiToken = client.ApiToken,
                IsEnabled = client.IsEnabled,
                SendPlatePreviewEnabled = client.SendPlatePreview,
                SendEveryPlateEnabled = client.SendEveryPlateEnabled,
                UserKey = client.UserKey,
            };
        }
    }
} 