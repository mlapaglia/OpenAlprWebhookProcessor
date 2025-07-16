using MediatR;
using OpenAlprWebhookProcessor.Alerts.Pushover;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetPushover
{
    public class GetPushoverQuery : IRequest<PushoverRequest>
    {
        public GetPushoverQuery()
        {
        }
    }
} 