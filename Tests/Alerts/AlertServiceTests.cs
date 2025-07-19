using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.ProcessorHub;

namespace Tests.Alerts
{
    [TestFixture]
    public class AlertServiceTests
    {
        private AlertService _alertService;
        private ILogger<AlertService> _logger;
        private IHubContext<ProcessorHub, IProcessorHub> _processorHub;
        private IProcessorHub _clientProxy;
        private IHubCallerClients<IProcessorHub> _clients;
        private IAlertClient _alertClient1;
        private IAlertClient _alertClient2;
        private List<IAlertClient> _alertClients;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger<AlertService>>();
            _processorHub = Substitute.For<IHubContext<ProcessorHub, IProcessorHub>>();
            _clientProxy = Substitute.For<IProcessorHub>();
            _clients = Substitute.For<IHubCallerClients<IProcessorHub>>();
            _alertClient1 = Substitute.For<IAlertClient>();
            _alertClient2 = Substitute.For<IAlertClient>();
            _alertClients = new List<IAlertClient> { _alertClient1, _alertClient2 };

            _clients.All.Returns(_clientProxy);
            _processorHub.Clients.Returns(_clients);

            _alertService = new AlertService(_logger, _processorHub, _alertClients);
        }

        [TearDown]
        public void TearDown()
        {
            // No cleanup needed
        }

        [Test]
        public void Constructor_WithValidParameters_ShouldInitializeService()
        {
            // Arrange & Act
            var service = new AlertService(_logger, _processorHub, _alertClients);

            // Assert
            service.Should().NotBeNull();
        }

        [Test]
        public void AddJob_WithValidAlert_ShouldAddToProcessingQueue()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = "ABC123",
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            // Act
            _alertService.AddJob(alertRequest);

            // Assert
            _logger.Received(1).LogInformation("adding job for alert: ");
        }

        [Test]
        public async Task StartAsync_ShouldStartProcessingWithoutError()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _alertService.StartAsync(cancellationToken);

            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task StopAsync_ShouldStopProcessingWithoutError()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _alertService.StopAsync(cancellationToken);

            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task ProcessAlertsAsync_WithValidAlert_ShouldProcessAlert()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = "ABC123",
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            // Act
            await _alertService.StartAsync(CancellationToken.None);
            _alertService.AddJob(alertRequest);
            
            // Give some time for processing
            await Task.Delay(100);
            await _alertService.StopAsync(CancellationToken.None);

            // Assert
            await _clientProxy.Received(1).LicensePlateAlerted("ABC123");
        }

        [Test]
        public async Task ProcessAlertsAsync_WithMultipleAlerts_ShouldProcessAllAlerts()
        {
            // Arrange
            var alertRequests = new[]
            {
                new AlertUpdateRequest
                {
                    PlateNumber = "ABC123",
                    Description = "Test alert 1",
                    IsUrgent = true,
                    PlateId = Guid.NewGuid(),
                    ReceivedOn = DateTimeOffset.Now
                },
                new AlertUpdateRequest
                {
                    PlateNumber = "XYZ789",
                    Description = "Test alert 2",
                    IsUrgent = false,
                    PlateId = Guid.NewGuid(),
                    ReceivedOn = DateTimeOffset.Now
                }
            };

            // Act
            await _alertService.StartAsync(CancellationToken.None);
            
            foreach (var alert in alertRequests)
            {
                _alertService.AddJob(alert);
            }
            
            // Give some time for processing
            await Task.Delay(200);
            await _alertService.StopAsync(CancellationToken.None);

            // Assert
            await _clientProxy.Received(1).LicensePlateAlerted("ABC123");

            await _clientProxy.Received(1).LicensePlateAlerted("XYZ789");
        }

        [Test]
        public async Task ProcessAlertsAsync_WithValidAlert_ShouldCallAllAlertClients()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = "ABC123",
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            // Act
            await _alertService.StartAsync(CancellationToken.None);
            _alertService.AddJob(alertRequest);
            
            // Give some time for processing
            await Task.Delay(100);
            await _alertService.StopAsync(CancellationToken.None);

            // Assert
            await _alertClient1.Received(1).SendAlertAsync(alertRequest, Arg.Any<CancellationToken>());
            await _alertClient2.Received(1).SendAlertAsync(alertRequest, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ProcessAlertsAsync_WithAlertClientThrowingException_ShouldLogErrorAndContinue()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = "ABC123",
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            var exception = new InvalidOperationException("Test exception");
            _alertClient1.SendAlertAsync(alertRequest, Arg.Any<CancellationToken>())
                .ThrowsAsync(exception);

            // Act
            await _alertService.StartAsync(CancellationToken.None);
            _alertService.AddJob(alertRequest);
            
            // Give some time for processing
            await Task.Delay(100);
            await _alertService.StopAsync(CancellationToken.None);

            // Assert
            _logger.Received(1).LogError(exception, "failed to send alert to alertClient");
            
            // Verify that the second alert client was still called despite the first one throwing
            await _alertClient2.Received(1).SendAlertAsync(alertRequest, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ProcessAlertsAsync_WithMultipleAlertClientsThrowingExceptions_ShouldLogAllErrorsAndContinue()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = "ABC123",
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            var exception1 = new InvalidOperationException("Test exception 1");
            var exception2 = new ArgumentException("Test exception 2");
            
            _alertClient1.SendAlertAsync(alertRequest, Arg.Any<CancellationToken>())
                .ThrowsAsync(exception1);
            _alertClient2.SendAlertAsync(alertRequest, Arg.Any<CancellationToken>())
                .ThrowsAsync(exception2);

            // Act
            await _alertService.StartAsync(CancellationToken.None);
            _alertService.AddJob(alertRequest);
            
            // Give some time for processing
            await Task.Delay(100);
            await _alertService.StopAsync(CancellationToken.None);

            // Assert
            _logger.Received(1).LogError(exception1, "failed to send alert to alertClient");
            _logger.Received(1).LogError(exception2, "failed to send alert to alertClient");
        }

        [Test]
        public async Task ProcessAlertsAsync_WithNoAlertClients_ShouldStillProcessSignalR()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = "ABC123",
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            var serviceWithNoClients = new AlertService(_logger, _processorHub, new List<IAlertClient>());

            // Act
            await serviceWithNoClients.StartAsync(CancellationToken.None);
            serviceWithNoClients.AddJob(alertRequest);
            
            // Give some time for processing
            await Task.Delay(100);
            await serviceWithNoClients.StopAsync(CancellationToken.None);

            // Assert
            await _clientProxy.Received(1).LicensePlateAlerted("ABC123");
        }

        [Test]
        public async Task ProcessAlertsAsync_WithEmptyPlateNumber_ShouldStillProcess()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = "",
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            // Act
            await _alertService.StartAsync(CancellationToken.None);
            _alertService.AddJob(alertRequest);
            
            // Give some time for processing
            await Task.Delay(100);
            await _alertService.StopAsync(CancellationToken.None);

            // Assert
            await _clientProxy.Received(1).LicensePlateAlerted("");
        }

        [Test]
        public async Task ProcessAlertsAsync_WithNullPlateNumber_ShouldStillProcess()
        {
            // Arrange
            var alertRequest = new AlertUpdateRequest
            {
                PlateNumber = null,
                Description = "Test alert",
                IsUrgent = true,
                PlateId = Guid.NewGuid(),
                ReceivedOn = DateTimeOffset.Now
            };

            // Act
            await _alertService.StartAsync(CancellationToken.None);
            _alertService.AddJob(alertRequest);
            
            // Give some time for processing
            await Task.Delay(100);
            await _alertService.StopAsync(CancellationToken.None);

            // Assert
            await _clientProxy.Received(1).LicensePlateAlerted(null);
        }

        [Test]
        public void AddJob_WithNullAlert_ShouldNotThrowException()
        {
            // Arrange
            AlertUpdateRequest alertRequest = null;

            // Act & Assert
            Assert.DoesNotThrow(() => _alertService.AddJob(alertRequest));
        }
    }
} 