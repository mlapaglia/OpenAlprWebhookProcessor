using MediatR;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Queries.GetWebhookStatus
{
    public class GetWebhookStatusQuery : IRequest<string>
    {
        public GetWebhookStatusQuery()
        {
        }
    }
} 