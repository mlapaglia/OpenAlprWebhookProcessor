using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace Tests.CameraUpdateService
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class TimerBasedBackgroundJobServiceTests
    {
        private TimerBasedBackgroundJobService _backgroundJobService;

        private IServiceProvider _serviceProvider;

        private IServiceScope _serviceScope;

        private IServiceScopeFactory _serviceScopeFactory;

        private ICameraUpdateService _cameraUpdateService;

        private ILogger<TimerBasedBackgroundJobService> _logger;

        [SetUp]
        public void SetUp()
        {
            _serviceProvider = Substitute.For<IServiceProvider>();
            _serviceScope = Substitute.For<IServiceScope>();
            _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            _cameraUpdateService = Substitute.For<ICameraUpdateService>();
            _logger = Substitute.For<ILogger<TimerBasedBackgroundJobService>>();

            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceScopeFactory.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.GetService(typeof(ICameraUpdateService)).Returns(_cameraUpdateService);

            _backgroundJobService = new TimerBasedBackgroundJobService(_serviceProvider, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            _backgroundJobService?.Dispose();
            _serviceScope?.Dispose();
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TimerBasedBackgroundJobService(null, _logger));
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TimerBasedBackgroundJobService(_serviceProvider, null));
        }

        [Test]
        public void Constructor_WithValidParameters_ShouldInitializeSuccessfully()
        {
            // Arrange & Act
            var service = new TimerBasedBackgroundJobService(_serviceProvider, _logger);

            // Assert
            service.Should().NotBeNull();
            service.Dispose(); // Clean up
        }

        [Test]
        public async Task EnqueueProcessJob_WithValidRequest_ShouldCallCameraUpdateService()
        {
            // Arrange
            var request = new CameraUpdateRequest
            {
                Id = Guid.NewGuid(),
                LicensePlate = "ABC123"
            };

            // Act
            await _backgroundJobService.EnqueueProcessJobAsync(request);

            // Give some time for the task to complete
            await Task.Delay(100);

            // Assert
            await _cameraUpdateService.Received(1).ProcessJobAsync(request);
        }

        [Test]
        public void EnqueueProcessJob_WithNullRequest_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrowAsync(() => _backgroundJobService.EnqueueProcessJobAsync(null));
        }

        [Test]
        public async Task EnqueueProcessJob_WithServiceException_ShouldHandleGracefully()
        {
            // Arrange
            var request = new CameraUpdateRequest
            {
                Id = Guid.NewGuid(),
                LicensePlate = "ABC123"
            };

            var exception = new InvalidOperationException("Test exception");
            _cameraUpdateService.ProcessJobAsync(request).ThrowsAsync(exception);

            // Act & Assert (should not throw)
            Assert.DoesNotThrowAsync(() => _backgroundJobService.EnqueueProcessJobAsync(request));

            // Give some time for the task to complete
            await Task.Delay(100);

            // Verify the service was called (and threw the exception)
            await _cameraUpdateService.Received(1).ProcessJobAsync(request);
        }

        [Test]
        public async Task EnqueueProcessSunriseSunsetJob_WithValidParameters_ShouldCallCameraUpdateService()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var sunriseSunset = SunriseSunset.Sunrise;
            var scheduleNextJob = true;

            // Act
            await _backgroundJobService.EnqueueProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);

            // Give some time for the task to complete
            await Task.Delay(100);

            // Assert
            await _cameraUpdateService.Received(1).ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);
        }

        [Test]
        public void EnqueueProcessSunriseSunsetJob_WithEmptyGuid_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrowAsync(() => _backgroundJobService.EnqueueProcessSunriseSunsetJobAsync(Guid.Empty, SunriseSunset.Sunrise, true));
        }

        [Test]
        public async Task EnqueueProcessSunriseSunsetJob_WithServiceException_ShouldHandleGracefully()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var sunriseSunset = SunriseSunset.Sunset;
            var scheduleNextJob = false;

            var exception = new InvalidOperationException("Test exception");
            _cameraUpdateService.ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob).ThrowsAsync(exception);

            // Act & Assert (should not throw)
            Assert.DoesNotThrowAsync(() => _backgroundJobService.EnqueueProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob));

            // Give some time for the task to complete
            await Task.Delay(100);

            // Verify the service was called (and threw the exception)
            await _cameraUpdateService.Received(1).ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);
        }

        [Test]
        public async Task ScheduleClearOverlayJob_WithValidParameters_ShouldReturnJobIdAndExecuteAfterDelay()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var delay = TimeSpan.FromMilliseconds(50);

            // Act
            var jobId = _backgroundJobService.ScheduleClearOverlayJob(cameraId, delay);

            // Assert
            jobId.Should().NotBeNullOrEmpty();

            // Wait for the job to execute
            await Task.Delay(delay.Add(TimeSpan.FromMilliseconds(50)));

            await _cameraUpdateService.Received(1).ClearExpiredOverlayAsync(cameraId);
        }

        [Test]
        public void ScheduleClearOverlayJob_WithEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            var delay = TimeSpan.FromMilliseconds(100);

            // Act
            var jobId = _backgroundJobService.ScheduleClearOverlayJob(Guid.Empty, delay);

            // Assert
            jobId.Should().BeNull();
        }

        [Test]
        public void ScheduleClearOverlayJob_WithNegativeDelay_ShouldReturnNull()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var delay = TimeSpan.FromMilliseconds(-100);

            // Act
            var jobId = _backgroundJobService.ScheduleClearOverlayJob(cameraId, delay);

            // Assert
            jobId.Should().BeNull();
        }

        [Test]
        public async Task ScheduleClearOverlayJob_WithServiceException_ShouldHandleGracefully()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var delay = TimeSpan.FromMilliseconds(50);

            var exception = new InvalidOperationException("Test exception");
            _cameraUpdateService.ClearExpiredOverlayAsync(cameraId).ThrowsAsync(exception);

            // Act
            var jobId = _backgroundJobService.ScheduleClearOverlayJob(cameraId, delay);

            // Assert
            jobId.Should().NotBeNullOrEmpty();

            // Wait for the job to execute
            await Task.Delay(delay.Add(TimeSpan.FromMilliseconds(50)));

            // Verify the service was called (and threw the exception)
            await _cameraUpdateService.Received(1).ClearExpiredOverlayAsync(cameraId);
        }

        [Test]
        public async Task ScheduleProcessSunriseSunsetJob_WithFutureTime_ShouldReturnJobIdAndExecuteAtScheduledTime()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var sunriseSunset = SunriseSunset.Sunrise;
            var scheduleNextJob = true;
            var scheduleAt = DateTimeOffset.Now.AddMilliseconds(50);

            // Act
            var jobId = await _backgroundJobService.ScheduleProcessSunriseSunsetJobAsync(
                cameraId,
                sunriseSunset,
                scheduleNextJob,
                scheduleAt);

            // Assert
            jobId.Should().NotBeNullOrEmpty();

            // Wait for the job to execute
            await Task.Delay(100);

            await _cameraUpdateService.Received(1).ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);
        }

        [Test]
        public async Task ScheduleProcessSunriseSunsetJob_WithPastTime_ShouldExecuteImmediately()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var sunriseSunset = SunriseSunset.Sunset;
            var scheduleNextJob = false;
            var scheduleAt = DateTimeOffset.Now.AddMilliseconds(-100);

            // Act
            var jobId = await _backgroundJobService.ScheduleProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob, scheduleAt);

            // Assert
            jobId.Should().BeNull();

            await Task.Delay(50);

            await _cameraUpdateService.DidNotReceive().ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);
        }

        [Test]
        public async Task ScheduleProcessSunriseSunsetJob_WithEmptyGuid_ShouldReturnNullAsync()
        {
            // Arrange
            var scheduleAt = DateTimeOffset.Now.AddMinutes(1);

            // Act
            var jobId = await _backgroundJobService.ScheduleProcessSunriseSunsetJobAsync(Guid.Empty, SunriseSunset.Sunrise, true, scheduleAt);

            // Assert
            jobId.Should().BeNull();
        }

        [Test]
        public async Task ScheduleProcessSunriseSunsetJob_WithServiceException_ShouldHandleGracefully()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var sunriseSunset = SunriseSunset.Sunrise;
            var scheduleNextJob = true;
            var scheduleAt = DateTimeOffset.Now.AddMilliseconds(50);

            var exception = new InvalidOperationException("Test exception");
            _cameraUpdateService.ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob).ThrowsAsync(exception);

            // Act
            var jobId = await _backgroundJobService.ScheduleProcessSunriseSunsetJobAsync(
                cameraId,
                sunriseSunset,
                scheduleNextJob,
                scheduleAt);

            // Assert
            jobId.Should().NotBeNullOrEmpty();

            // Wait for the job to execute
            await Task.Delay(100);

            // Verify the service was called (and threw the exception)
            await _cameraUpdateService.Received(1).ProcessSunriseSunsetJobAsync(
                cameraId,
                sunriseSunset,
                scheduleNextJob);
        }

        [Test]
        public async Task DeleteJob_WithValidJobId_ShouldCancelScheduledJob()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var delay = TimeSpan.FromMilliseconds(200);

            var jobId = _backgroundJobService.ScheduleClearOverlayJob(cameraId, delay);

            // Act
            _backgroundJobService.DeleteJob(jobId);

            // Wait longer than the original delay
            await Task.Delay(300);

            // Assert
            await _cameraUpdateService.DidNotReceive().ClearExpiredOverlayAsync(cameraId);
        }

        [Test]
        public void DeleteJob_WithNullJobId_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _backgroundJobService.DeleteJob(null));
        }

        [Test]
        public void DeleteJob_WithEmptyJobId_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _backgroundJobService.DeleteJob(string.Empty));
        }

        [Test]
        public void DeleteJob_WithInvalidJobId_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _backgroundJobService.DeleteJob("invalid-job-id"));
        }

        [Test]
        public async Task Dispose_ShouldCancelAllScheduledJobs()
        {
            // Arrange
            var cameraId1 = Guid.NewGuid();
            var cameraId2 = Guid.NewGuid();
            var delay = TimeSpan.FromMilliseconds(200);

            _backgroundJobService.ScheduleClearOverlayJob(cameraId1, delay);
            _backgroundJobService.ScheduleClearOverlayJob(cameraId2, delay);

            // Act
            _backgroundJobService.Dispose();

            // Wait longer than the original delay
            await Task.Delay(300);

            // Assert
            await _cameraUpdateService.DidNotReceive().ClearExpiredOverlayAsync(cameraId1);
            await _cameraUpdateService.DidNotReceive().ClearExpiredOverlayAsync(cameraId2);
        }

        [Test]
        public void Dispose_CalledMultipleTimes_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                _backgroundJobService.Dispose();
                _backgroundJobService.Dispose();
            });
        }

        [Test]
        public async Task MultipleJobsScheduled_ShouldAllExecuteCorrectly()
        {
            // Arrange
            var cameraId1 = Guid.NewGuid();
            var cameraId2 = Guid.NewGuid();
            var delay1 = TimeSpan.FromMilliseconds(50);
            var delay2 = TimeSpan.FromMilliseconds(100);

            // Act
            var jobId1 = _backgroundJobService.ScheduleClearOverlayJob(cameraId1, delay1);
            var jobId2 = _backgroundJobService.ScheduleClearOverlayJob(cameraId2, delay2);

            // Wait for both jobs to execute
            await Task.Delay(150);

            // Assert
            jobId1.Should().NotBeNullOrEmpty();
            jobId2.Should().NotBeNullOrEmpty();
            jobId1.Should().NotBe(jobId2);

            await _cameraUpdateService.Received(1).ClearExpiredOverlayAsync(cameraId1);
            await _cameraUpdateService.Received(1).ClearExpiredOverlayAsync(cameraId2);
        }

        [Test]
        public async Task ScheduledJobAfterDispose_ShouldNotExecute()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var delay = TimeSpan.FromMilliseconds(100);

            // Act
            _backgroundJobService.ScheduleClearOverlayJob(cameraId, delay);
            _backgroundJobService.Dispose();

            // Wait longer than the delay
            await Task.Delay(200);

            // Assert
            await _cameraUpdateService.DidNotReceive().ClearExpiredOverlayAsync(cameraId);
        }
    }
} 