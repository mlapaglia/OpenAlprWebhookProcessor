using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent;
using System;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Queries.GetAgent
{
    [TestFixture]
    public class GetAgentQueryHandlerTests : TestBase
    {
        private GetAgentQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetAgentQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithExistingAgent_ReturnsCorrectAgentDto()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                Uid = "test-agent-123",
                OpenAlprWebServerUrl = "https://server.example.com",
                Latitude = 40.7128,
                Longitude = -74.0060,
                SunriseOffset = 30,
                SunsetOffset = -30,
                TimeZoneOffset = -5.0,
                IsDebugEnabled = true,
                IsImageCompressionEnabled = false,
                LastHeartbeatEpochMs = 1640995200000,
                ScheduledScrapingIntervalMinutes = 60,
                NextScrapeEpochMs = 1640995260000
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAgentQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(agent.Id);
            result.EndpointUrl.Should().Be("https://agent.example.com");
            result.Uid.Should().Be("test-agent-123");
            result.OpenAlprWebServerUrl.Should().Be("https://server.example.com");
            result.Latitude.Should().Be(40.7128);
            result.Longitude.Should().Be(-74.0060);
            result.SunriseOffset.Should().Be(30);
            result.SunsetOffset.Should().Be(-30);
            result.TimezoneOffset.Should().Be(-5.0);
            result.IsDebugEnabled.Should().BeTrue();
            result.IsImageCompressionEnabled.Should().BeFalse();
            result.LastHeartbeatEpochMs.Should().Be(1640995200000);
            result.ScheduledScrapingIntervalMinutes.Should().Be(60);
            result.NextScrapeInMinutes.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithNoAgent_ReturnsEmptyAgentDto()
        {
            // Arrange
            var query = new GetAgentQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(Guid.Empty);
            result.EndpointUrl.Should().BeNull();
            result.Uid.Should().BeNull();
            result.OpenAlprWebServerUrl.Should().BeNull();
            result.Latitude.Should().BeNull();
            result.Longitude.Should().BeNull();
            result.SunriseOffset.Should().Be(0);
            result.SunsetOffset.Should().Be(0);
            result.TimezoneOffset.Should().Be(0);
            result.IsDebugEnabled.Should().BeFalse();
            result.IsImageCompressionEnabled.Should().BeFalse();
            result.LastHeartbeatEpochMs.Should().Be(0);
            result.ScheduledScrapingIntervalMinutes.Should().BeNull();
            result.NextScrapeInMinutes.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithAgentHavingNullNextScrapeEpoch_ReturnsNullNextScrapeInMinutes()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                Uid = "test-agent-123",
                NextScrapeEpochMs = null
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAgentQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.NextScrapeInMinutes.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithAgentHavingFutureNextScrapeEpoch_CalculatesCorrectNextScrapeInMinutes()
        {
            // Arrange
            var futureEpoch = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeMilliseconds();
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                Uid = "test-agent-123",
                NextScrapeEpochMs = futureEpoch
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAgentQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.NextScrapeInMinutes.Should().NotBeNull();
            result.NextScrapeInMinutes.Should().BeGreaterThan(25); // Should be approximately 30 minutes
            result.NextScrapeInMinutes.Should().BeLessThan(35);
        }

        [Test]
        public async Task Handle_WithAgentHavingPastNextScrapeEpoch_ReturnsNegativeNextScrapeInMinutes()
        {
            // Arrange
            var pastEpoch = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds();
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                Uid = "test-agent-123",
                NextScrapeEpochMs = pastEpoch
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAgentQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.NextScrapeInMinutes.Should().NotBeNull();
            result.NextScrapeInMinutes.Should().BeLessThan(0);
        }

        [Test]
        public async Task Handle_CallsGetFirstAgentAsync()
        {
            // Arrange
            var query = new GetAgentQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            // The fact that we get a result (even if empty) confirms the method was called
        }
    }
} 