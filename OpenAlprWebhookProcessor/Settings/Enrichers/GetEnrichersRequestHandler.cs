using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.Enrichers
{
    public class GetEnrichersRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetEnrichersRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<Enricher> HandleAsync(CancellationToken cancellationToken)
        {
            var enricher = await _processorContext.Enrichers.FirstOrDefaultAsync(cancellationToken);

            if (enricher == null)
            {
                return new Enricher();
            }

            return new Enricher()
            {
                ApiKey = enricher.ApiKey,
                EnricherType = enricher.EnricherType,
                IsEnabled = enricher.IsEnabled,
                Id = enricher.Id,
                RunAlways = enricher.RunAlways,
                RunAtNight = enricher.RunAtNight,
                RunManually = enricher.RunManually,
            };
        }
    }
}
