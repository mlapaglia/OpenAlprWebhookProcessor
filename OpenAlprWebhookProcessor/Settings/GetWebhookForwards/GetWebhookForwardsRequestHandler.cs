using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    public class GetWebhookForwardsRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetWebhookForwardsRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<WebhookForward>> HandleAsync(CancellationToken cancellationToken)
        {
            var webhookForwards = await _processorContext.WebhookForwards.ToListAsync(cancellationToken);

            var forwards = new List<WebhookForward>();

            foreach (var webhook in webhookForwards)
            {
                var forward = new WebhookForward()
                {
                    Destination = webhook.FowardingDestination,
                    Id = webhook.Id,
                    IgnoreSslErrors = webhook.IgnoreSslErrors,
                    ForwardGroupPreviews = webhook.ForwardGroupPreviews,
                    ForwardSinglePlates = webhook.ForwardSinglePlates,
                    ForwardGroups = webhook.ForwardGroups,
                };

                forwards.Add(forward);
            }

            return forwards;
        }
    }
}
