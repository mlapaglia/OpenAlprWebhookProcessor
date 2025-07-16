using MediatR;
using OpenAlprWebhookProcessor.Alerts.WebPush;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetWebPush
{
    public class GetWebPushQuery : IRequest<WebPushRequest>
    {
        public GetWebPushQuery()
        {
        }
    }
} 