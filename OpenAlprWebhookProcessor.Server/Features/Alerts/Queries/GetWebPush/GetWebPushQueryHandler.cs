using MediatR;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetWebPush
{
    public class GetWebPushQueryHandler : IRequestHandler<GetWebPushQuery, WebPushRequest>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GetWebPushClientRequestHandler _getWebPushClientRequestHandler;

        public GetWebPushQueryHandler(
            IUnitOfWork unitOfWork,
            GetWebPushClientRequestHandler getWebPushClientRequestHandler)
        {
            _unitOfWork = unitOfWork;
            _getWebPushClientRequestHandler = getWebPushClientRequestHandler;
        }

        public async Task<WebPushRequest> Handle(GetWebPushQuery request, CancellationToken cancellationToken)
        {
            return await _getWebPushClientRequestHandler.HandleAsync(cancellationToken);
        }
    }
} 