using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprAgentScraper
{
    public interface IOpenAlprAgentScraper
    {
        Task<long> ScrapeAgentAsync(long lastSuccessfulScrapeEpoch, string agentEndpointUrl, CancellationToken cancellationToken);
        Task ScrapeAgentImagesAsync(List<string> plateGroupIds, CancellationToken cancellationToken);
    }
}