using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Queries.GetWebPushPublicKey
{
    public class GetWebPushPublicKeyQueryHandler : IRequestHandler<GetWebPushPublicKeyQuery, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWebPushPublicKeyQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(GetWebPushPublicKeyQuery request, CancellationToken cancellationToken = default)
        {
            var keys = await VapidKeyHelper.GetVapidKeysAsync(_unitOfWork, cancellationToken);
            return keys.PublicKey;
        }
    }
} 