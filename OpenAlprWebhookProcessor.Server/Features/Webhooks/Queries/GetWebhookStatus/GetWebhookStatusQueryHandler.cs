using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Queries.GetWebhookStatus
{
    public class GetWebhookStatusQueryHandler : IQueryHandler<GetWebhookStatusQuery, string>
    {
        public GetWebhookStatusQueryHandler()
        {
        }

        public async ValueTask<string> Handle(GetWebhookStatusQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult("Webhook Processor");
        }
    }
} 