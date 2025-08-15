using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprAgentScraper
{
    public interface IOpenAlprAgentScraper
    {
        Task ScrapeAgentAsync(CancellationToken cancellationToken = default);
        Task ScrapeAgentImagesAsync(CancellationToken cancellationToken = default);
    }
} 