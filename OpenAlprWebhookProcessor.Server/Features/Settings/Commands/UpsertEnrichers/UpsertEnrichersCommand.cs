using MediatR;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertEnrichers
{
    public class UpsertEnrichersCommand : IRequest
    {
        public EnricherDto Enricher { get; set; }

        public UpsertEnrichersCommand(EnricherDto enricher)
        {
            Enricher = enricher;
        }
    }
} 