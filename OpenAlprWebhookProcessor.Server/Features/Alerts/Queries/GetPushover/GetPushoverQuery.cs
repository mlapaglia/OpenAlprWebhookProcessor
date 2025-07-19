using MediatR;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetPushover
{
    public class GetPushoverQuery : IRequest<PushoverRequest>
    {
        public GetPushoverQuery()
        {
        }
    }
} 