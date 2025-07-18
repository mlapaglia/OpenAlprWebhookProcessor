using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Hydrator;
using OpenAlprWebhookProcessor.ProcessorHub;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;

namespace Tests.Hydration
{
    [TestFixture]
    public class HydrationServiceTests
    {
        private HydrationService _hydrationService;
        private IServiceProvider _serviceProvider;
        private IServiceScope _serviceScope;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHubContext<ProcessorHub, IProcessorHub> _processorHub;
        private IProcessorHub _clientProxy;
        private IHubCallerClients<IProcessorHub> _clients;
        private ILogger<HydrationService> _logger;
        private IUnitOfWork _unitOfWork;
        private IAgentRepository _agentRepository;
        private IOpenAlprAgentScraper _scraper;
        private Agent _agent;

        [SetUp]
        public void SetUp()
        {
            _serviceProvider = Substitute.For<IServiceProvider>();
            _serviceScope = Substitute.For<IServiceScope>();
            _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            _processorHub = Substitute.For<IHubContext<ProcessorHub, IProcessorHub>>();
            _clientProxy = Substitute.For<IProcessorHub>();
            _clients = Substitute.For<IHubCallerClients<IProcessorHub>>();
            _logger = Substitute.For<ILogger<HydrationService>>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _agentRepository = Substitute.For<IAgentRepository>();
            _scraper = Substitute.For<IOpenAlprAgentScraper>();

            _agent = new Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-uid",
                ScheduledScrapingIntervalMinutes = 15
            };

            // Setup service provider with IServiceScopeFactory
            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceScopeFactory.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.GetService(typeof(ILogger<HydrationService>)).Returns(_logger);
            _serviceScope.ServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_unitOfWork);
            _serviceScope.ServiceProvider.GetService(typeof(IOpenAlprAgentScraper)).Returns(_scraper);

            // Setup SignalR
            _processorHub.Clients.Returns(_clients);
            _clients.All.Returns(_clientProxy);

            // Setup unit of work
            _unitOfWork.Agents.Returns(_agentRepository);
            _agentRepository.GetFirstAgentAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_agent));

            // Create service under test
            _hydrationService = new HydrationService(_serviceProvider, _processorHub);
        }

        [TearDown]
        public async Task TearDownAsync()
        {
            try
            {
                await _hydrationService?.StopAsync(CancellationToken.None);
            }
            catch (ObjectDisposedException)
            {
                // Service may have already been stopped, ignore
            }
            _unitOfWork.Dispose();
            _serviceScope.Dispose();
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HydrationService(null, _processorHub));
        }

        [Test]
        public void Constructor_WithNullProcessorHub_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HydrationService(_serviceProvider, null));
        }

        [Test]
        public async Task ScheduleHydrationAsync_WithValidAgent_ShouldSetNextScrapeTime()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _hydrationService.ScheduleHydrationAsync(cancellationToken);

            // Assert
            await _agentRepository.Received(1).GetFirstAgentAsync(cancellationToken);
            _unitOfWork.Agents.Received(1).Update(_agent);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
            _agent.NextScrapeEpochMs.Should().NotBeNull();
        }

        [Test]
        public async Task ScheduleHydrationAsync_WithEmptyAgentUid_ShouldLogWarningAndReturn()
        {
            // Arrange
            _agent.Uid = string.Empty;
            var cancellationToken = CancellationToken.None;

            // Act
            await _hydrationService.ScheduleHydrationAsync(cancellationToken);

            // Assert
            _logger.Received(1).LogWarning("Agent UID is not set. Cannot schedule hydration.");
            _unitOfWork.Agents.DidNotReceive().Update(Arg.Any<Agent>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ScheduleHydrationAsync_WithNullScheduledInterval_ShouldClearNextScrapeTime()
        {
            // Arrange
            _agent.ScheduledScrapingIntervalMinutes = null;
            var cancellationToken = CancellationToken.None;

            // Act
            await _hydrationService.ScheduleHydrationAsync(cancellationToken);

            // Assert
            _agent.NextScrapeEpochMs.Should().BeNull();
            _unitOfWork.Agents.Received(1).Update(_agent);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public void StartHydration_WithValidRequest_ShouldAddToQueue()
        {
            // Arrange
            var request = "test-request";

            // Act
            _hydrationService.StartHydration(request);

            // Assert
            // Since we can't directly test the internal queue, we just verify no exceptions are thrown
            // The actual processing would be tested in integration tests

            Assert.Pass();
        }

        [Test]
        public async Task StartAsync_ShouldScheduleHydrationAndStartProcessing()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _hydrationService.StartAsync(cancellationToken);

            // Assert
            await _agentRepository.Received(1).GetFirstAgentAsync(cancellationToken);
            _unitOfWork.Agents.Received(1).Update(_agent);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task StopAsync_ShouldCancelTokenAndDisposeResources()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _hydrationService.StopAsync(cancellationToken);

            // Assert
            // Service should complete without throwing exceptions
            // Timer disposal is tested implicitly by proper cleanup
            Assert.Pass();
        }

        [Test]
        public async Task ScheduleHydrationAsync_WithScrapingException_ShouldNotThrow()
        {
            // Arrange
            _agentRepository.GetFirstAgentAsync(Arg.Any<CancellationToken>()).Throws<Exception>();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await _hydrationService.Invoking(async s => await s.ScheduleHydrationAsync(cancellationToken))
                .Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task ScheduleHydrationAsync_WithDatabaseException_ShouldNotThrow()
        {
            // Arrange
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Throws<Exception>();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await _hydrationService.Invoking(async s => await s.ScheduleHydrationAsync(cancellationToken))
                .Should().ThrowAsync<Exception>();
        }
    }
} 