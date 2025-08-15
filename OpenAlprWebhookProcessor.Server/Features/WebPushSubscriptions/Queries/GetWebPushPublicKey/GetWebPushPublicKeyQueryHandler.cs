using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.VapidKeys;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Queries.GetWebPushPublicKey
{
    public class GetWebPushPublicKeyQueryHandler : IQueryHandler<GetWebPushPublicKeyQuery, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWebPushPublicKeyQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<string> Handle(GetWebPushPublicKeyQuery request, CancellationToken cancellationToken = default)
        {
            var keys = await VapidKeyHelper.GetVapidKeysAsync(_unitOfWork, cancellationToken);
            return keys.PublicKey;
        }
    }
} 