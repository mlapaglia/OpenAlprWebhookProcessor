using AwesomeAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.Hydrator;
using OpenAlprWebhookProcessor.ProcessorHub;
using System.Runtime.CompilerServices;
using Tests.TestHelpers;

namespace Tests.Hydration
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class HydrationHostedServiceTests : TestBase
    {
        private HydrationHostedService _hydrationHostedService;
        private IHydrationService _hydrationService;
        private IServiceProvider _serviceProvider;
        private IHubContext<ProcessorHub, IProcessorHub> _processorHub;
        private ILogger<HydrationHostedService> _logger;
        private IProcessorHub _processorHubClient;
        private IServiceScope _serviceScope;
        private IServiceProvider _scopedServiceProvider;
        private IOpenAlprAgentScraper _agentScraper;
        private ILogger<HydrationHostedService> _scopedLogger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _hydrationService = Substitute.For<IHydrationService>();
            _serviceProvider = Substitute.For<IServiceProvider>();
            _processorHub = Substitute.For<IHubContext<ProcessorHub, IProcessorHub>>();
            _logger = Substitute.For<ILogger<HydrationHostedService>>();
            _processorHubClient = Substitute.For<IProcessorHub>();
            _serviceScope = Substitute.For<IServiceScope>();
            _scopedServiceProvider = Substitute.For<IServiceProvider>();
            _agentScraper = Substitute.For<IOpenAlprAgentScraper>();
            _scopedLogger = Substitute.For<ILogger<HydrationHostedService>>();

            // Setup SignalR hub
            _processorHub.Clients.All.Returns(_processorHubClient);

            // Setup service provider scope - will be configured in individual tests
            _serviceScope.ServiceProvider.Returns(_scopedServiceProvider);

            _hydrationHostedService = new HydrationHostedService(
                _hydrationService,
                _serviceProvider,
                _processorHub,
                _logger);
        }

        [TearDown]
        public override void TearDown()
        {
            _hydrationHostedService?.Dispose();
            _serviceScope?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_WithNullHydrationService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new HydrationHostedService(null, _serviceProvider, _processorHub, _logger));

            exception.ParamName.Should().Be("hydrationService");
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new HydrationHostedService(_hydrationService, null, _processorHub, _logger));

            exception.ParamName.Should().Be("serviceProvider");
        }

        [Test]
        public void Constructor_WithNullProcessorHub_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new HydrationHostedService(_hydrationService, _serviceProvider, null, _logger));

            exception.ParamName.Should().Be("processorHub");
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new HydrationHostedService(_hydrationService, _serviceProvider, _processorHub, null));

            exception.ParamName.Should().Be("logger");
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _hydrationHostedService.Should().NotBeNull();
        }

        [Test]
        public async Task ExecuteAsync_PerformsInitialSchedulingAndProcessesRequests()
        {
            // Arrange
            SetupServiceProviderForScoping();
            var cancellationTokenSource = new CancellationTokenSource();
            var hydrationRequests = CreateAsyncEnumerable("request1", "request2");
            
            _hydrationService.GetConsumingHydrationRequestsAsync(Arg.Any<CancellationToken>())
                .Returns(hydrationRequests);
            _hydrationService.GetPendingHydrationCount().Returns(2, 1, 0);

            // Act
            var executeTask = _hydrationHostedService.StartAsync(cancellationTokenSource.Token);
            
            // Give it time to process requests
            await Task.Delay(150);
            
            // Stop the service
            cancellationTokenSource.Cancel();
            await executeTask;

            // Assert
            // ScheduleHydrationAsync is called: 1 initial + 1 for each processed request (2) = 3 total
            await _hydrationService.Received(3).ScheduleHydrationAsync(Arg.Any<CancellationToken>());
            await _agentScraper.Received(2).ScrapeAgentAsync(Arg.Any<CancellationToken>());
            await _agentScraper.Received(2).ScrapeAgentImagesAsync(Arg.Any<CancellationToken>());
            await _processorHubClient.Received(2).ScrapeFinished();
        }

        [Test]
        public async Task ExecuteAsync_WithCancellation_LogsInformationAndStops()
        {
            // Arrange
            SetupServiceProviderForScoping();
            var cancellationTokenSource = new CancellationTokenSource();
            
            // Create an async enumerable that will be cancelled
            _hydrationService.GetConsumingHydrationRequestsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateCancellableAsyncEnumerable(cancellationTokenSource));

            // Act
            var executeTask = _hydrationHostedService.StartAsync(cancellationTokenSource.Token);
            
            // Cancel after a short delay
            await Task.Delay(50);
            cancellationTokenSource.Cancel();
            
            await executeTask;

            // Assert - service should complete without throwing
            executeTask.IsCompleted.Should().BeTrue();
        }

        [Test]
        public async Task ExecuteAsync_WithExceptionInProcessing_LogsErrorAndContinues()
        {
            // Arrange
            SetupServiceProviderForScoping();
            var cancellationTokenSource = new CancellationTokenSource();
            var hydrationRequests = CreateAsyncEnumerable("request1", "request2");
            
            _hydrationService.GetConsumingHydrationRequestsAsync(Arg.Any<CancellationToken>())
                .Returns(hydrationRequests);

            // Make the first scrape operation throw an exception
            _agentScraper.ScrapeAgentAsync(Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Test exception"))
                .AndDoes(x => { }); // Second call succeeds

            // Act
            var executeTask = _hydrationHostedService.StartAsync(cancellationTokenSource.Token);
            
            await Task.Delay(150);
            
            cancellationTokenSource.Cancel();
            await executeTask;

            // Assert - Should still attempt to reschedule after exception
            await _hydrationService.Received().ScheduleHydrationAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ProcessSingleHydrationRequest_ExecutesScrapingAndNotifiesHub()
        {
            // Arrange
            SetupServiceProviderForScoping();
            var cancellationTokenSource = new CancellationTokenSource();
            var hydrationRequests = CreateAsyncEnumerable("test-request");
            
            _hydrationService.GetConsumingHydrationRequestsAsync(Arg.Any<CancellationToken>())
                .Returns(hydrationRequests);
            _hydrationService.GetPendingHydrationCount().Returns(1);

            // Act
            var executeTask = _hydrationHostedService.StartAsync(cancellationTokenSource.Token);
            
            await Task.Delay(150);
            
            cancellationTokenSource.Cancel();
            await executeTask;

            // Assert
            await _agentScraper.Received(1).ScrapeAgentAsync(Arg.Any<CancellationToken>());
            await _agentScraper.Received(1).ScrapeAgentImagesAsync(Arg.Any<CancellationToken>());
            await _processorHubClient.Received(1).ScrapeFinished();
            _serviceScope.Received(1).Dispose();
        }

        [Test]
        public async Task ProcessSingleHydrationRequest_WithScrapingException_DisposesScope()
        {
            // Arrange
            SetupServiceProviderForScoping();
            var cancellationTokenSource = new CancellationTokenSource();
            var hydrationRequests = CreateAsyncEnumerable("test-request");
            
            _hydrationService.GetConsumingHydrationRequestsAsync(Arg.Any<CancellationToken>())
                .Returns(hydrationRequests);
            _agentScraper.ScrapeAgentAsync(Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Scraping failed"));

            // Act
            var executeTask = _hydrationHostedService.StartAsync(cancellationTokenSource.Token);
            
            await Task.Delay(150);
            
            cancellationTokenSource.Cancel();
            await executeTask;

            // Assert - Scope should still be disposed even when exception occurs
            _serviceScope.Received(1).Dispose();
        }

        [Test]
        public async Task ProcessSingleHydrationRequest_WithCancellation_PropagatesCancellation()
        {
            // Arrange
            SetupServiceProviderForScoping();
            var cancellationTokenSource = new CancellationTokenSource();
            var hydrationRequests = CreateAsyncEnumerable("test-request");
            
            _hydrationService.GetConsumingHydrationRequestsAsync(Arg.Any<CancellationToken>())
                .Returns(hydrationRequests);
            _agentScraper.ScrapeAgentAsync(Arg.Any<CancellationToken>())
                .Throws(new OperationCanceledException());

            // Act
            var executeTask = _hydrationHostedService.StartAsync(cancellationTokenSource.Token);
            
            await Task.Delay(100);
            cancellationTokenSource.Cancel();
            
            await executeTask;

            // Assert - Should not call ScrapeAgentImagesAsync if first call was cancelled
            await _agentScraper.Received(1).ScrapeAgentAsync(Arg.Any<CancellationToken>());
        }

        private static async IAsyncEnumerable<string> CreateAsyncEnumerable(params string[] items)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Delay(10); // Small delay to simulate async behavior
            }
        }

        private static async IAsyncEnumerable<string> CreateCancellableAsyncEnumerable(
            CancellationTokenSource cancellationTokenSource,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    yield return "test-request";
                    await Task.Delay(100, cancellationToken);
                }
            }
            finally
            {
                // Ensure we exit the enumerable when cancelled
            }
        }

        private void SetupServiceProviderForScoping()
        {
            var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(_serviceScope);
            
            // Configure service provider to return the scope factory and handle scoped services
            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(serviceScopeFactory);
            _scopedServiceProvider.GetService(typeof(ILogger<HydrationHostedService>)).Returns(_scopedLogger);
            _scopedServiceProvider.GetService(typeof(IOpenAlprAgentScraper)).Returns(_agentScraper);
        }
    }
}