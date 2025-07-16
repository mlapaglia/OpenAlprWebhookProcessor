using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper
{
    public interface IOpenAlprAgentScraper
    {
        Task ScrapeAgentAsync(CancellationToken cancellationToken);
        Task ScrapeAgentImagesAsync(CancellationToken cancellationToken);
    }
} 