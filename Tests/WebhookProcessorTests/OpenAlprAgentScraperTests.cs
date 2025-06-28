using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenAlprWebhookProcessor.Server.WebhookProcessor;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprAgentScraper;

namespace Tests.WebhookProcessorTests
{
    public class OpenAlprAgentScraperTests
    {
        private readonly IGroupWebhookHandler _groupHandlerSub;

        private readonly ILogger<OpenAlprAgentScraper> _loggerSub;

        private readonly IImageRetrieverService _imageRetrieverSub;

        private readonly OpenAlprAgentScraper _scraper;

        public OpenAlprAgentScraperTests()
        {
            _groupHandlerSub = Substitute.For<IGroupWebhookHandler>();

            _loggerSub = Substitute.For<ILogger<OpenAlprAgentScraper>>();
            _imageRetrieverSub = Substitute.For<IImageRetrieverService>();

            _scraper = new OpenAlprAgentScraper(
                _groupHandlerSub,
                _loggerSub,
                _imageRetrieverSub
            );
        }

        [Test]
        public async Task ScrapeAgentImagesAsync_AddsJobsForMissingImages()
        {
            var plateGroupIds = new List<string>()
            {
                Guid.NewGuid().ToString(),
            };

            await _scraper.ScrapeAgentImagesAsync(
                plateGroupIds,
                CancellationToken.None);

            _imageRetrieverSub.Received(1).TryAddJob(plateGroupIds[0]);
        }
    }
}