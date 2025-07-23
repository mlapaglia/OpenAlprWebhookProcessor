using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService
{
    [TestFixture]
    public class CameraUpdateHostedServiceTests : TestBase
    {
        private CameraUpdateHostedService _cameraUpdateHostedService;
        private ICameraUpdateService _cameraUpdateService;
        private ILogger<CameraUpdateHostedService> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _cameraUpdateService = Substitute.For<ICameraUpdateService>();
            _logger = Substitute.For<ILogger<CameraUpdateHostedService>>();
            
            _cameraUpdateHostedService = new CameraUpdateHostedService(
                _cameraUpdateService,
                _logger);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Constructor_WithNullCameraUpdateService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new CameraUpdateHostedService(null, _logger));
            
            ex.ParamName.Should().Be("cameraUpdateService");
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new CameraUpdateHostedService(_cameraUpdateService, null));
            
            ex.ParamName.Should().Be("logger");
        }

        [Test]
        public async Task StartAsync_SuccessfullySchedulesDayNightTasks()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _cameraUpdateHostedService.StartAsync(cancellationToken);

            // Assert
            await _cameraUpdateService.Received(1).ScheduleDayNightTaskAsync();
        }

        [Test]
        public async Task StartAsync_WhenSchedulingFails_DoesNotThrow()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedException = new InvalidOperationException("Scheduling failed");
            
            _cameraUpdateService.ScheduleDayNightTaskAsync()
                .Throws(expectedException);

            // Act & Assert - should not throw
            await _cameraUpdateHostedService.StartAsync(cancellationToken);

            // Verify the method was still called
            await _cameraUpdateService.Received(1).ScheduleDayNightTaskAsync();
        }

        [Test]
        public async Task StopAsync_SuccessfullyClearsOverlays()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _cameraUpdateHostedService.StopAsync(cancellationToken);

            // Assert
            await _cameraUpdateService.Received(1).ForceClearOverlaysAsync(cancellationToken);
        }

        [Test]
        public async Task StopAsync_WhenClearingOverlaysFails_DoesNotThrow()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedException = new InvalidOperationException("Clearing overlays failed");
            
            _cameraUpdateService.ForceClearOverlaysAsync(cancellationToken)
                .Throws(expectedException);

            // Act & Assert - should not throw
            await _cameraUpdateHostedService.StopAsync(cancellationToken);

            // Verify the method was still called
            await _cameraUpdateService.Received(1).ForceClearOverlaysAsync(cancellationToken);
        }

        [Test]
        public async Task StartAsync_PassesCancellationTokenCorrectly()
        {
            // Arrange
            var cancellationToken = new CancellationToken(true); // Already cancelled

            // Act
            await _cameraUpdateHostedService.StartAsync(cancellationToken);

            // Assert - method should still be called even with cancelled token
            await _cameraUpdateService.Received(1).ScheduleDayNightTaskAsync();
        }

        [Test]
        public async Task StopAsync_PassesCancellationTokenCorrectly()
        {
            // Arrange
            var cancellationToken = new CancellationToken(true); // Already cancelled

            // Act
            await _cameraUpdateHostedService.StopAsync(cancellationToken);

            // Assert - method should be called with the correct cancellation token
            await _cameraUpdateService.Received(1).ForceClearOverlaysAsync(cancellationToken);
        }

        [Test]
        public async Task StartAsync_MultipleCallsWork()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act - call multiple times
            await _cameraUpdateHostedService.StartAsync(cancellationToken);
            await _cameraUpdateHostedService.StartAsync(cancellationToken);
            await _cameraUpdateHostedService.StartAsync(cancellationToken);

            // Assert - should be called multiple times
            await _cameraUpdateService.Received(3).ScheduleDayNightTaskAsync();
        }

        [Test]
        public async Task StopAsync_MultipleCallsWork()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act - call multiple times
            await _cameraUpdateHostedService.StopAsync(cancellationToken);
            await _cameraUpdateHostedService.StopAsync(cancellationToken);
            await _cameraUpdateHostedService.StopAsync(cancellationToken);

            // Assert - should be called multiple times
            await _cameraUpdateService.Received(3).ForceClearOverlaysAsync(cancellationToken);
        }

        [Test]
        public async Task StartAsync_ThenStopAsync_BothMethodsCalled()
        {
            // Arrange
            var startToken = new CancellationToken();
            var stopToken = new CancellationToken();

            // Act
            await _cameraUpdateHostedService.StartAsync(startToken);
            await _cameraUpdateHostedService.StopAsync(stopToken);

            // Assert
            await _cameraUpdateService.Received(1).ScheduleDayNightTaskAsync();
            await _cameraUpdateService.Received(1).ForceClearOverlaysAsync(stopToken);
        }

        [Test]
        public async Task StartAsync_WithTaskCancelledException_DoesNotThrow()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var cancellationException = new OperationCanceledException("Operation was cancelled");
            
            _cameraUpdateService.ScheduleDayNightTaskAsync()
                .Throws(cancellationException);

            // Act & Assert - should not throw
            await _cameraUpdateHostedService.StartAsync(cancellationToken);

            // Verify the method was still called
            await _cameraUpdateService.Received(1).ScheduleDayNightTaskAsync();
        }

        [Test]
        public async Task StopAsync_WithTaskCancelledException_DoesNotThrow()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var cancellationException = new OperationCanceledException("Operation was cancelled");
            
            _cameraUpdateService.ForceClearOverlaysAsync(cancellationToken)
                .Throws(cancellationException);

            // Act & Assert - should not throw
            await _cameraUpdateHostedService.StopAsync(cancellationToken);

            // Verify the method was still called
            await _cameraUpdateService.Received(1).ForceClearOverlaysAsync(cancellationToken);
        }
    }
} 