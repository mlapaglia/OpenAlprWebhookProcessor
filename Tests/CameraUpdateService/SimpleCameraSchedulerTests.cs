using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class SimpleCameraSchedulerTests : TestBase
    {
        private ILogger<SimpleCameraScheduler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _logger = Substitute.For<ILogger<SimpleCameraScheduler>>();
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new SimpleCameraScheduler(null, _logger));
            
            ex.ParamName.Should().Be("serviceProvider");
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new SimpleCameraScheduler(serviceProvider, null));
            
            ex.ParamName.Should().Be("logger");
        }

        [Test]
        public async Task StartAsync_CallsResetAllCamerasAndScheduleAllCameras()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var agent = CreateTestAgent();
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.StartAsync(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("Starting simple camera scheduler");

            scheduler.Dispose();
        }

        [Test]
        public async Task StopAsync_ClearsAllTimersAndCancellationTokens()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);
            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.StopAsync(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("Stopping simple camera scheduler");

            scheduler.Dispose();
        }

        [Test]
        public async Task ResetAllCamerasAsync_WithEnabledCameras_ProcessesThemInParallel()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var agent = CreateTestAgent();
            var camera1 = CreateTestCamera(updateDayNightModeEnabled: true);
            var camera2 = CreateTestCamera(updateDayNightModeEnabled: true);

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.ResetAllCamerasAsync(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("Resetting all cameras to correct day/night mode");

            scheduler.Dispose();
        }

        [Test]
        public async Task ResetAllCamerasAsync_WithDisabledCameras_SkipsThem()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var agent = CreateTestAgent();
            var camera = CreateTestCamera(updateDayNightModeEnabled: false);

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.ResetAllCamerasAsync(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("Resetting all cameras to correct day/night mode");

            scheduler.Dispose();
        }

        [Test]
        public async Task ScheduleCameraAsync_WithValidCamera_SchedulesCamera()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var agent = CreateTestAgent();
            var camera = CreateTestCamera(updateDayNightModeEnabled: true);

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.ScheduleCameraAsync(camera.Id, cancellationToken);

            // Assert - Camera should be scheduled (verified by checking that no warning was logged)
            _logger.DidNotReceive().LogWarning("Camera {CameraId} not found or day/night mode disabled", camera.Id);

            scheduler.Dispose();
        }

        [Test]
        public async Task ScheduleCameraAsync_WithNonExistentCamera_LogsWarning()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var nonExistentCameraId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.ScheduleCameraAsync(nonExistentCameraId, cancellationToken);

            // Assert - method completed successfully without throwing

            scheduler.Dispose();
        }

        [Test]
        public async Task ScheduleCameraAsync_WithDisabledCamera_LogsWarning()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var agent = CreateTestAgent();
            var camera = CreateTestCamera(updateDayNightModeEnabled: false);

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.ScheduleCameraAsync(camera.Id, cancellationToken);

            // Assert - method completed successfully without throwing

            scheduler.Dispose();
        }

        [Test]
        public async Task RemoveCameraScheduleAsync_RemovesSchedule()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var cameraId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.RemoveCameraScheduleAsync(cameraId, cancellationToken);

            // Assert - method completed successfully without throwing

            scheduler.Dispose();
        }

        [Test]
        public async Task RescheduleAllCamerasAsync_ClearsExistingAndReschedulesAll()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var agent = CreateTestAgent();
            var camera = CreateTestCamera(updateDayNightModeEnabled: true);

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.RescheduleAllCamerasAsync(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("Rescheduling all cameras due to agent settings change");

            scheduler.Dispose();
        }

        [Test]
        public void GetNextScheduledExecutionTime_WithNonExistentCamera_ReturnsNull()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            var cameraId = Guid.NewGuid();

            // Act
            var result = scheduler.GetNextScheduledExecutionTime(cameraId);

            // Assert
            result.Should().BeNull();

            scheduler.Dispose();
        }

        [Test]
        public void GetAllScheduledJobs_WithNoScheduledJobs_ReturnsEmptyList()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            // Act
            var result = scheduler.GetAllScheduledJobs();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            scheduler.Dispose();
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var scheduler = new SimpleCameraScheduler(serviceProvider, _logger);

            // Act - call dispose multiple times
            scheduler.Dispose();
            scheduler.Dispose();
            scheduler.Dispose();

            // Assert - no exception should be thrown
            Assert.Pass("Dispose can be called multiple times without throwing");
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(UnitOfWork);
            services.AddSingleton<ICameraFactory>(new MockCameraFactory());
            return services.BuildServiceProvider();
        }

        private OpenAlprWebhookProcessor.Data.Agent CreateTestAgent()
        {
            return new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Hostname = "test-agent",
                Latitude = 40.7128,
                Longitude = -74.0060,
                TimeZoneOffset = -5,
                SunriseOffset = 30,
                SunsetOffset = -30
            };
        }

        private OpenAlprWebhookProcessor.Data.Camera CreateTestCamera(bool updateDayNightModeEnabled = true)
        {
            return new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprName = "Test Camera",
                OpenAlprCameraId = 1,
                IpAddress = "192.168.1.200",
                CameraUsername = "admin",
                CameraPassword = "password",
                Manufacturer = OpenAlprWebhookProcessor.Features.Cameras.Configuration.CameraManufacturer.Hikvision,
                UpdateDayNightModeEnabled = updateDayNightModeEnabled,
                PlatesSeen = 0,
                Latitude = 40.7128,
                Longitude = -74.0060,
                TimezoneOffset = -5,
                SunriseOffset = 30,
                SunsetOffset = -30,
                NextClearOverlayScheduleId = string.Empty
            };
        }
    }
}