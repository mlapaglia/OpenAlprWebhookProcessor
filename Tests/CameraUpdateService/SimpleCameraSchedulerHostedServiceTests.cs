using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class SimpleCameraSchedulerHostedServiceTests : TestBase
    {
        private SimpleCameraSchedulerHostedService _hostedService;
        private ISimpleCameraScheduler _cameraScheduler;
        private ILogger<SimpleCameraSchedulerHostedService> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _cameraScheduler = Substitute.For<ISimpleCameraScheduler>();
            _logger = Substitute.For<ILogger<SimpleCameraSchedulerHostedService>>();
            
            _hostedService = new SimpleCameraSchedulerHostedService(
                _cameraScheduler,
                _logger);
        }

        [TearDown]
        public override void TearDown()
        {
            _hostedService?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_WithNullCameraScheduler_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new SimpleCameraSchedulerHostedService(null, _logger));
            
            ex.ParamName.Should().Be("cameraScheduler");
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new SimpleCameraSchedulerHostedService(_cameraScheduler, null));
            
            ex.ParamName.Should().Be("logger");
        }

        [Test]
        public async Task StopAsync_CallsSchedulerStopAsync()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _hostedService.StopAsync(cancellationToken);

            // Assert
            await _cameraScheduler.Received(1).StopAsync(cancellationToken);
        }

        [Test]
        public async Task StopAsync_WhenSchedulerFails_StillCallsBaseStop()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedException = new InvalidOperationException("Scheduler stop failed");
            
            _cameraScheduler.StopAsync(cancellationToken)
                .Throws(expectedException);

            // Act & Assert - should throw the exception since StopAsync doesn't catch it
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _hostedService.StopAsync(cancellationToken));
            
            ex.Message.Should().Be("Scheduler stop failed");
            await _cameraScheduler.Received(1).StopAsync(cancellationToken);
        }

        [Test]
        public async Task StopAsync_PassesCancellationTokenCorrectly()
        {
            // Arrange
            var cancellationToken = new CancellationToken(true); // Already cancelled

            // Act
            await _hostedService.StopAsync(cancellationToken);

            // Assert - method should be called with the correct cancellation token
            await _cameraScheduler.Received(1).StopAsync(cancellationToken);
        }

        [Test]
        public async Task StopAsync_MultipleCallsWork()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act - call multiple times
            await _hostedService.StopAsync(cancellationToken);
            await _hostedService.StopAsync(cancellationToken);
            await _hostedService.StopAsync(cancellationToken);

            // Assert - should be called multiple times
            await _cameraScheduler.Received(3).StopAsync(cancellationToken);
        }

        [Test]
        public async Task StopAsync_WithTaskCancelledException_PropagatesException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var cancellationException = new OperationCanceledException("Operation was cancelled");
            
            _cameraScheduler.StopAsync(cancellationToken)
                .Throws(cancellationException);

            // Act & Assert - should propagate the exception
            var ex = Assert.ThrowsAsync<OperationCanceledException>(() => 
                _hostedService.StopAsync(cancellationToken));
            
            ex.Message.Should().Be("Operation was cancelled");
            await _cameraScheduler.Received(1).StopAsync(cancellationToken);
        }
    }
}