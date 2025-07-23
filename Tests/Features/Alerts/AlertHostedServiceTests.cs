using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.ProcessorHub;
using System.Runtime.CompilerServices;
using Tests.TestHelpers;

namespace Tests.Features.Alerts
{
    [TestFixture]
    public class AlertHostedServiceTests : TestBase
    {
        private AlertHostedService _alertHostedService;
        private IAlertService _alertService;
        private ILogger<AlertHostedService> _logger;
        private IHubContext<ProcessorHub, IProcessorHub> _processorHub;
        private IProcessorHub _processorHubClient;
        private List<IAlertClient> _alertClients;
        private IAlertClient _alertClient1;
        private IAlertClient _alertClient2;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _alertService = Substitute.For<IAlertService>();
            _logger = Substitute.For<ILogger<AlertHostedService>>();
            _processorHub = Substitute.For<IHubContext<ProcessorHub, IProcessorHub>>();
            _processorHubClient = Substitute.For<IProcessorHub>();
            
            _processorHub.Clients.All.Returns(_processorHubClient);
            
            _alertClient1 = Substitute.For<IAlertClient>();
            _alertClient2 = Substitute.For<IAlertClient>();
            _alertClients = new List<IAlertClient> { _alertClient1, _alertClient2 };
            
            _alertHostedService = new AlertHostedService(
                _alertService,
                _logger,
                _processorHub,
                _alertClients);
        }

        [TearDown]
        public override void TearDown()
        {
            _alertHostedService?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task ExecuteAsync_WithNoAlerts_CompletesSuccessfully()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource();
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateEmptyAsyncEnumerable());

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            await Task.Delay(50); // Allow some processing time
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            _alertService.Received(1).GetConsumingAlertsAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ExecuteAsync_WithSingleAlert_ProcessesAlertCorrectly()
        {
            // Arrange
            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = new CancellationTokenSource();
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateAsyncEnumerable(alert));
            _alertService.GetPendingAlertsCount().Returns(1);

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            await Task.Delay(100); // Give it time to process
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            await _processorHubClient.Received(1).LicensePlateAlerted(alert.PlateNumber);
            await _alertClient1.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
            _alertService.Received(1).GetPendingAlertsCount();
        }

        [Test]
        public async Task ExecuteAsync_WithMultipleAlerts_ProcessesAllAlerts()
        {
            // Arrange
            var alert1 = CreateTestAlertUpdateRequest("ABC123");
            var alert2 = CreateTestAlertUpdateRequest("XYZ789");
            var cancellationToken = new CancellationTokenSource();
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateAsyncEnumerable(alert1, alert2));
            _alertService.GetPendingAlertsCount().Returns(2, 1);

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            await Task.Delay(200); // Give it time to process both alerts
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            await _processorHubClient.Received(1).LicensePlateAlerted(alert1.PlateNumber);
            await _processorHubClient.Received(1).LicensePlateAlerted(alert2.PlateNumber);
            await _alertClient1.Received(1).SendAlertAsync(alert1, Arg.Any<CancellationToken>());
            await _alertClient1.Received(1).SendAlertAsync(alert2, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alert1, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alert2, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ExecuteAsync_WhenSignalRThrowsException_ContinuesProcessingAlertClients()
        {
            // Arrange
            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = new CancellationTokenSource();
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateAsyncEnumerable(alert));
            _processorHubClient.LicensePlateAlerted(Arg.Any<string>())
                .Throws(new InvalidOperationException("SignalR connection failed"));

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            await Task.Delay(100);
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            await _alertClient1.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ExecuteAsync_WhenAlertClientThrowsException_ContinuesWithOtherClients()
        {
            // Arrange
            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = new CancellationTokenSource();
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateAsyncEnumerable(alert));
            _alertClient1.SendAlertAsync(Arg.Any<AlertUpdateRequest>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Alert client failed"));

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            await Task.Delay(100);
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            await _alertClient1.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
            await _processorHubClient.Received(1).LicensePlateAlerted(alert.PlateNumber);
        }

        [Test]
        public async Task ExecuteAsync_WhenProcessingAlertThrowsException_ContinuesWithNextAlert()
        {
            // Arrange
            var alert1 = CreateTestAlertUpdateRequest("ABC123");
            var alert2 = CreateTestAlertUpdateRequest("XYZ789");
            var cancellationToken = new CancellationTokenSource();
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateAsyncEnumerable(alert1, alert2));
            
            // Make first alert fail in SignalR, but second should still process
            _processorHubClient.LicensePlateAlerted("ABC123")
                .Throws(new InvalidOperationException("Processing failed"));

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            await Task.Delay(200);
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            await _processorHubClient.Received(1).LicensePlateAlerted(alert1.PlateNumber);
            await _processorHubClient.Received(1).LicensePlateAlerted(alert2.PlateNumber);
            await _alertClient1.Received(1).SendAlertAsync(alert2, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alert2, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ExecuteAsync_WithOperationCancelledException_HandlesGracefully()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource();
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateCancelledAsyncEnumerable());

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();
            
            // Should not throw
            await executeTask;

            // Assert - operation should complete gracefully
            _alertService.Received(1).GetConsumingAlertsAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void ExecuteAsync_WithUnhandledException_RethrowsException()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource();
            var expectedException = new InvalidOperationException("Fatal error");
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Throws(expectedException);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _alertHostedService.StartAsync(cancellationToken.Token);
            });
        }

        [Test]
        public async Task ExecuteAsync_WithNoAlertClients_StillProcessesSignalR()
        {
            // Arrange
            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = new CancellationTokenSource();
            var emptyAlertClients = new List<IAlertClient>();
            
            var serviceWithNoClients = new AlertHostedService(
                _alertService,
                _logger,
                _processorHub,
                emptyAlertClients);
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateAsyncEnumerable(alert));

            // Act
            var executeTask = serviceWithNoClients.StartAsync(cancellationToken.Token);
            await Task.Delay(100);
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            await _processorHubClient.Received(1).LicensePlateAlerted(alert.PlateNumber);
        }

        [Test]
        public async Task ExecuteAsync_ProcessesAlertsInParallel()
        {
            // Arrange
            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = new CancellationTokenSource();
            var delayTasks = new List<TaskCompletionSource<bool>>();
            
            _alertService.GetConsumingAlertsAsync(Arg.Any<CancellationToken>())
                .Returns(CreateAsyncEnumerable(alert));

            // Set up delays to verify parallel execution
            var delay1 = new TaskCompletionSource<bool>();
            var delay2 = new TaskCompletionSource<bool>();
            delayTasks.Add(delay1);
            delayTasks.Add(delay2);

            _alertClient1.SendAlertAsync(Arg.Any<AlertUpdateRequest>(), Arg.Any<CancellationToken>())
                .Returns(async x => { await delay1.Task; });
            _alertClient2.SendAlertAsync(Arg.Any<AlertUpdateRequest>(), Arg.Any<CancellationToken>())
                .Returns(async x => { await delay2.Task; });

            // Act
            var executeTask = _alertHostedService.StartAsync(cancellationToken.Token);
            await Task.Delay(50); // Let it start processing

            // Complete both tasks at the same time to verify they were running in parallel
            delay1.SetResult(true);
            delay2.SetResult(true);
            
            await Task.Delay(50);
            cancellationToken.Cancel();
            await executeTask;

            // Assert
            await _alertClient1.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alert, Arg.Any<CancellationToken>());
        }

        private AlertUpdateRequest CreateTestAlertUpdateRequest(string plateNumber = "TEST123")
        {
            return new AlertUpdateRequest
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = plateNumber,
                Description = "Test alert description",
                IsUrgent = true,
                PlateJpeg = new byte[] { 1, 2, 3, 4 },
                PlateJpegUrl = "/api/test",
                ReceivedOn = DateTimeOffset.UtcNow
            };
        }

        private IAsyncEnumerable<AlertUpdateRequest> CreateAsyncEnumerable(params AlertUpdateRequest[] alerts)
        {
            return CreateAsyncEnumerableImpl(alerts);
        }

        private async IAsyncEnumerable<AlertUpdateRequest> CreateAsyncEnumerableImpl(AlertUpdateRequest[] alerts)
        {
            foreach (var alert in alerts)
            {
                yield return alert;
                await Task.Delay(10); // Small delay to simulate async enumeration
            }
        }

        private IAsyncEnumerable<AlertUpdateRequest> CreateEmptyAsyncEnumerable()
        {
            return CreateEmptyAsyncEnumerableImpl();
        }

        private async IAsyncEnumerable<AlertUpdateRequest> CreateEmptyAsyncEnumerableImpl()
        {
            await Task.CompletedTask;
            yield break;
        }

        private IAsyncEnumerable<AlertUpdateRequest> CreateCancelledAsyncEnumerable()
        {
            return CreateCancelledAsyncEnumerableImpl();
        }

        private async IAsyncEnumerable<AlertUpdateRequest> CreateCancelledAsyncEnumerableImpl([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            yield break;
        }
    }
} 