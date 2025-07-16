using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Queries.GetWebhookStatus
{
    public class GetWebhookStatusQueryHandler : IRequestHandler<GetWebhookStatusQuery, string>
    {
        public GetWebhookStatusQueryHandler()
        {
        }

        public async Task<string> Handle(GetWebhookStatusQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult("Webhook Processor");
        }
    }
} 