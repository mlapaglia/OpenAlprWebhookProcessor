using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Hydrator;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.UpsertAgent
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpsertAgentCommandHandlerTests : TestBase
    {
        private UpsertAgentCommandHandler _handler;
        private IImageRetrieverService _imageRetrieverService;
        private IHydrationService _hydrationService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _imageRetrieverService = Substitute.For<IImageRetrieverService>();
            _hydrationService = Substitute.For<IHydrationService>();
            var cameraScheduler = Substitute.For<ISimpleCameraScheduler>();
            _handler = new UpsertAgentCommandHandler(UnitOfWork, _imageRetrieverService, _hydrationService, cameraScheduler);
        }

        [Test]
        public async Task Handle_NewAgent_AddsAgentToDatabase()
        {
            // Arrange
            var agentDto = new AgentDto
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                Uid = "test-agent-123",
                OpenAlprWebServerUrl = "https://server.example.com",
                Latitude = 40.7128,
                Longitude = -74.0060,
                SunriseOffset = 30,
                SunsetOffset = -30,
                TimezoneOffset = -5.0,
                IsDebugEnabled = true,
                IsImageCompressionEnabled = false,
                ScheduledScrapingIntervalMinutes = 60
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAgents = await UnitOfWork.Agents.GetAllAsync();
            dbAgents.Should().HaveCount(1);

            var dbAgent = dbAgents.First();
            dbAgent.EndpointUrl.Should().Be("https://agent.example.com");
            dbAgent.Uid.Should().Be("test-agent-123");
            dbAgent.IsDebugEnabled.Should().BeTrue();
            dbAgent.IsImageCompressionEnabled.Should().BeFalse();
            dbAgent.ScheduledScrapingIntervalMinutes.Should().Be(60);
        }

        [Test]
        public async Task Handle_ExistingAgent_UpdatesAgentInDatabase()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var existingAgent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = agentId,
                EndpointUrl = "https://old.example.com",
                Uid = "old-agent-123",
                IsDebugEnabled = false,
                IsImageCompressionEnabled = false,
                ScheduledScrapingIntervalMinutes = 30
            };
            await UnitOfWork.Agents.AddAsync(existingAgent);
            await UnitOfWork.SaveChangesAsync();

            var agentDto = new AgentDto
            {
                Id = agentId,
                EndpointUrl = "https://new.example.com",
                Uid = "new-agent-123",
                IsDebugEnabled = true,
                IsImageCompressionEnabled = true,
                ScheduledScrapingIntervalMinutes = 120
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAgents = await UnitOfWork.Agents.GetAllAsync();
            dbAgents.Should().HaveCount(1);

            var dbAgent = dbAgents.First();
            dbAgent.Id.Should().Be(agentId);
            dbAgent.EndpointUrl.Should().Be("https://new.example.com");
            dbAgent.Uid.Should().Be("new-agent-123");
            dbAgent.IsDebugEnabled.Should().BeTrue();
            dbAgent.IsImageCompressionEnabled.Should().BeTrue();
            dbAgent.ScheduledScrapingIntervalMinutes.Should().Be(120);
        }

        [Test]
        public async Task Handle_EnableImageCompression_CallsImageRetrieverService()
        {
            // Arrange
            var agentDto = new AgentDto
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                IsImageCompressionEnabled = true
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            _imageRetrieverService.Received(1).AddImageCompressionJob();
        }

        [Test]
        public async Task Handle_DisableImageCompression_DoesNotCallImageRetrieverService()
        {
            // Arrange
            var agentDto = new AgentDto
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                IsImageCompressionEnabled = false
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            _imageRetrieverService.DidNotReceive().AddImageCompressionJob();
        }

        [Test]
        public async Task Handle_ExistingAgentWithCompressionEnabled_EnableImageCompression_CallsImageRetrieverService()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var existingAgent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = agentId,
                EndpointUrl = "https://agent.example.com",
                IsImageCompressionEnabled = false
            };
            await UnitOfWork.Agents.AddAsync(existingAgent);
            await UnitOfWork.SaveChangesAsync();

            var agentDto = new AgentDto
            {
                Id = agentId,
                EndpointUrl = "https://agent.example.com",
                IsImageCompressionEnabled = true
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            _imageRetrieverService.Received(1).AddImageCompressionJob();
        }

        [Test]
        public async Task Handle_ExistingAgentWithCompressionEnabled_KeepImageCompression_DoesNotCallImageRetrieverService()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var existingAgent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = agentId,
                EndpointUrl = "https://agent.example.com",
                IsImageCompressionEnabled = true
            };
            await UnitOfWork.Agents.AddAsync(existingAgent);
            await UnitOfWork.SaveChangesAsync();

            var agentDto = new AgentDto
            {
                Id = agentId,
                EndpointUrl = "https://agent.example.com",
                IsImageCompressionEnabled = true
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            _imageRetrieverService.DidNotReceive().AddImageCompressionJob();
        }

        [Test]
        public async Task Handle_WithScheduledScrapingInterval_CallsHydrationService()
        {
            // Arrange
            var agentDto = new AgentDto
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                ScheduledScrapingIntervalMinutes = 60
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            await _hydrationService.Received(1).ScheduleHydrationAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithoutScheduledScrapingInterval_DoesNotCallHydrationService()
        {
            // Arrange
            var agentDto = new AgentDto
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                ScheduledScrapingIntervalMinutes = null
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            await _hydrationService.DidNotReceive().ScheduleHydrationAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidAgent_SavesChanges()
        {
            // Arrange
            var agentDto = new AgentDto
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "https://agent.example.com",
                Uid = "test-agent-123"
            };
            var command = new UpsertAgentCommand(agentDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAgents = await UnitOfWork.Agents.GetAllAsync();
            dbAgents.Should().HaveCount(1);
        }
    }
} 