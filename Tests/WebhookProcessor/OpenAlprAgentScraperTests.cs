using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;
using System.Net;
using System.Text;
using System.Text.Json;
using Tests.TestHelpers;
namespace Tests.WebhookProcessor
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class OpenAlprAgentScraperTests : TestBase
    {
        private OpenAlprAgentScraper _scraper;
        private IGroupWebhookHandler _groupWebhookHandler;
        private IImageRetrieverService _imageRetrieverService;
        private ITimeService _timeService;
        private HttpClient _httpClient;
        private TestHttpMessageHandler _httpMessageHandler;
        private ILogger<OpenAlprAgentScraper> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _groupWebhookHandler = Substitute.For<IGroupWebhookHandler>();
            _imageRetrieverService = Substitute.For<IImageRetrieverService>();
            _timeService = Substitute.For<ITimeService>();
            _logger = Substitute.For<ILogger<OpenAlprAgentScraper>>();
            
            _httpMessageHandler = new TestHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler);
            
            _scraper = new OpenAlprAgentScraper(
                _groupWebhookHandler,
                Context,
                _logger,
                _imageRetrieverService,
                _httpClient,
                _timeService);
        }

        [TearDown]
        public override void TearDown()
        {
            _httpClient?.Dispose();
            _httpMessageHandler?.Dispose();
            base.TearDown();
        }

        #region ScrapeAgentAsync Tests

        [Test]
        public async Task ScrapeAgentAsync_FirstScrapeWithZeroEpoch_GetsEarliestEpoch()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            agent.LastSuccessfulScrapeEpoch = 0;
            
            await Context.Agents.AddAsync(agent);
            await Context.SaveChangesAsync();

            var earliestEpoch = 1609459200000; // Jan 1, 2021
            var currentTime = earliestEpoch + 1000;

            _timeService.UtcNowMilliseconds.Returns(currentTime);
            
            // Mock the earliest epoch response
            var earliestResponse = $"<html>Earliest date epoch: {earliestEpoch}</html>";
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(earliestResponse));
            
            // Mock empty metadata response for the scraping endpoint
            var emptyMetadata = "[]";
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(emptyMetadata));

            var cancellationToken = GetCancellationToken();

            // Act
            await _scraper.ScrapeAgentAsync(cancellationToken);

            // Assert
            var updatedAgent = await Context.Agents.FirstAsync();
            updatedAgent.LastSuccessfulScrapeEpoch.Should().Be(currentTime);
        }

        [Test]
        public async Task ScrapeAgentAsync_NoNewDataToScrape_CompletesWithoutError()
        {
            // Arrange
            var currentTime = 1609459200000; // Jan 1, 2021
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            agent.LastSuccessfulScrapeEpoch = currentTime;
            
            await Context.Agents.AddAsync(agent);
            await Context.SaveChangesAsync();

            _timeService.UtcNowMilliseconds.Returns(currentTime);
            
            var cancellationToken = GetCancellationToken();

            // Act
            await _scraper.ScrapeAgentAsync(cancellationToken);

            // Assert
            var updatedAgent = await Context.Agents.FirstAsync();
            updatedAgent.LastSuccessfulScrapeEpoch.Should().Be(currentTime);
        }

        [Test]
        public async Task ScrapeAgentAsync_WithValidMetadata_ProcessesSuccessfully()
        {
            // Arrange
            var currentTime = 1609459200000 + 86400000 + 1000; // 1 day + 1 second later
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            agent.LastSuccessfulScrapeEpoch = 1609459200000;
            
            await Context.Agents.AddAsync(agent);
            await Context.SaveChangesAsync();

            _timeService.UtcNowMilliseconds.Returns(currentTime);

            var metadata = new List<ScrapeMetadata>
            {
                new ScrapeMetadata { Key = "test-key-1", Time = "2021-01-01T12:00:00Z" },
                new ScrapeMetadata { Key = "test-key-2", Time = "2021-01-01T12:05:00Z" }
            };

            var metadataJson = JsonSerializer.Serialize(metadata);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(metadataJson));

            var group = CreateTestGroup();
            var groupJson = JsonSerializer.Serialize(group);
            // Add response for each metadata item
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(groupJson));
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(groupJson));
            
            // Add empty response for second iteration of the scraper loop
            var emptyMetadata = new List<ScrapeMetadata>();
            var emptyMetadataJson = JsonSerializer.Serialize(emptyMetadata);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(emptyMetadataJson));

            var cancellationToken = GetCancellationToken();

            // Act
            await _scraper.ScrapeAgentAsync(cancellationToken);

            // Assert
            await _groupWebhookHandler.Received().HandleWebhookAsync(
                Arg.Any<Webhook>(),
                true,
                cancellationToken);

            var updatedAgent = await Context.Agents.FirstAsync();
            updatedAgent.LastSuccessfulScrapeEpoch.Should().Be(currentTime);
        }

        [Test]
        public async Task ScrapeAgentAsync_HttpErrorFromMetadataEndpoint_ContinuesProcessing()
        {
            // Arrange
            var currentTime = 1609459200000 + 86400000 + 1000;
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            agent.LastSuccessfulScrapeEpoch = 1609459200000;
            
            await Context.Agents.AddAsync(agent);
            await Context.SaveChangesAsync();

            _timeService.UtcNowMilliseconds.Returns(currentTime);

            // First call returns error
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError, Encoding.UTF8.GetBytes("Server Error"));

            var cancellationToken = GetCancellationToken();

            // Act
            await _scraper.ScrapeAgentAsync(cancellationToken);

            // Assert
            await _groupWebhookHandler.DidNotReceive().HandleWebhookAsync(
                Arg.Any<Webhook>(),
                Arg.Any<bool>(),
                cancellationToken);

            var updatedAgent = await Context.Agents.FirstAsync();
            updatedAgent.LastSuccessfulScrapeEpoch.Should().Be(currentTime);
        }

        [Test]
        public async Task ScrapeAgentAsync_GroupWebhookHandlerThrows_ContinuesProcessing()
        {
            // Arrange
            var currentTime = 1609459200000 + 86400000 + 1000;
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            agent.LastSuccessfulScrapeEpoch = 1609459200000;
            
            await Context.Agents.AddAsync(agent);
            await Context.SaveChangesAsync();

            _timeService.UtcNowMilliseconds.Returns(currentTime);

            var metadata = new List<ScrapeMetadata>
            {
                new ScrapeMetadata { Key = "test-key-1", Time = "2021-01-01T12:00:00Z" }
            };

            var metadataJson = JsonSerializer.Serialize(metadata);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(metadataJson));

            var group = CreateTestGroup();
            var groupJson = JsonSerializer.Serialize(group);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(groupJson));

            // Add empty response for second iteration of the scraper loop
            var emptyMetadata = new List<ScrapeMetadata>();
            var emptyMetadataJson = JsonSerializer.Serialize(emptyMetadata);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(emptyMetadataJson));

            _groupWebhookHandler.HandleWebhookAsync(
                Arg.Any<Webhook>(),
                true,
                Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Webhook processing failed"));

            var cancellationToken = GetCancellationToken();

            // Act
            await _scraper.ScrapeAgentAsync(cancellationToken);

            // Assert
            await _groupWebhookHandler.Received().HandleWebhookAsync(
                Arg.Any<Webhook>(),
                true,
                cancellationToken);

            var updatedAgent = await Context.Agents.FirstAsync();
            updatedAgent.LastSuccessfulScrapeEpoch.Should().Be(currentTime);
        }

        #endregion

        #region ScrapeAgentImagesAsync Tests

        [Test]
        public async Task ScrapeAgentImagesAsync_WithPlatesNeedingImages_AddsJobsForEach()
        {
            // Arrange
            var plateGroup1 = TestDataFactory.CreateTestPlateGroupForImageRelay("uuid-1");
            var plateGroup2 = TestDataFactory.CreateTestPlateGroupForImageRelay("uuid-2");
            var plateGroup3 = TestDataFactory.CreateTestPlateGroupForImageRelay("uuid-3");
            
            // Set AgentImageScrapeOccurredOn to null to indicate they need images
            plateGroup1.AgentImageScrapeOccurredOn = null;
            plateGroup2.AgentImageScrapeOccurredOn = null;
            plateGroup3.AgentImageScrapeOccurredOn = 1609459200000; // Already processed

            await Context.PlateGroups.AddRangeAsync(plateGroup1, plateGroup2, plateGroup3);
            await Context.SaveChangesAsync();

            var cancellationToken = GetCancellationToken();

            // Act
            await _scraper.ScrapeAgentImagesAsync(cancellationToken);

            // Assert
            _imageRetrieverService.Received(1).AddImageRetrievalJob("uuid-1");
            _imageRetrieverService.Received(1).AddImageRetrievalJob("uuid-2");
            _imageRetrieverService.DidNotReceive().AddImageRetrievalJob("uuid-3");
        }

        [Test]
        public async Task ScrapeAgentImagesAsync_NoPlatesNeedingImages_NoJobsAdded()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay("uuid-1");
            plateGroup.AgentImageScrapeOccurredOn = 1609459200000; // Already processed

            await Context.PlateGroups.AddAsync(plateGroup);
            await Context.SaveChangesAsync();

            var cancellationToken = GetCancellationToken();

            // Act
            await _scraper.ScrapeAgentImagesAsync(cancellationToken);

            // Assert
            _imageRetrieverService.DidNotReceive().AddImageRetrievalJob(Arg.Any<string>());
        }

        #endregion

        #region Helper Methods

        private Group CreateTestGroup()
        {
            return new Group
            {
                EpochStart = 1609459200000,
                EpochEnd = 1609459205000,
                CameraId = 1,
                BestUuid = "test-uuid-123",
                BestPlateNumber = "ABC123",
                Candidates = new List<Candidate>
                {
                    new Candidate { Plate = "ABC123", Confidence = 95.5 }
                },
                BestPlate = new Plate
                {
                    Coordinates = new List<Coordinate>
                    {
                        new Coordinate { X = 100, Y = 200 },
                        new Coordinate { X = 300, Y = 200 },
                        new Coordinate { X = 300, Y = 400 },
                        new Coordinate { X = 100, Y = 400 }
                    },
                    Confidence = 95.5,
                    ProcessingTimeMs = 150.0
                }
            };
        }

        #endregion

        #region Test Helper Class

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> _responses = new();
            public string LastRequestUri { get; private set; }

            public void SetupResponse(HttpStatusCode statusCode, byte[] content = null)
            {
                var response = new HttpResponseMessage(statusCode);
                if (content != null)
                {
                    response.Content = new ByteArrayContent(content);
                }
                _responses.Enqueue(response);
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequestUri = request.RequestUri.ToString();
                
                if (_responses.Count > 0)
                {
                    return Task.FromResult(_responses.Dequeue());
                }
                
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        #endregion
    }
} 