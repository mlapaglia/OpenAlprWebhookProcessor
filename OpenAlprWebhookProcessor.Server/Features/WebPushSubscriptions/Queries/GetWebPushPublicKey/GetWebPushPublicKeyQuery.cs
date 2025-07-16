using MediatR;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Queries.GetWebPushPublicKey
{
    public class GetWebPushPublicKeyQuery : IRequest<string>
    {
        public GetWebPushPublicKeyQuery()
        {
        }
    }
} 