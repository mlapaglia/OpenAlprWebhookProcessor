using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.WebhookProcessor;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprAgentScraper;

namespace Tests.WebhookProcessorTests
{
    public class OpenAlprAgentScraperTests
    {
        private readonly IGroupWebhookHandler _groupHandlerSub;
        private readonly ProcessorContextFactory<ProcessorContext> _processContextFactory;
        private readonly ILogger<OpenAlprAgentScraper> _loggerSub;
        private readonly IImageRetrieverService _imageRetrieverSub;
        private readonly OpenAlprAgentScraper _scraper;

        public OpenAlprAgentScraperTests()
        {
            _groupHandlerSub = Substitute.For<IGroupWebhookHandler>();
            _processContextFactory = new ProcessorContextFactory<ProcessorContext>(opts => new ProcessorContext(opts));

            _loggerSub = Substitute.For<ILogger<OpenAlprAgentScraper>>();
            _imageRetrieverSub = Substitute.For<IImageRetrieverService>();

            _scraper = new OpenAlprAgentScraper(
                _groupHandlerSub,
                _loggerSub,
                _imageRetrieverSub
            );
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _processContextFactory.Dispose();
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