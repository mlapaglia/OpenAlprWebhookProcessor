using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CameraSchedulingTests : TestBase
    {
        private SimpleCameraScheduler _scheduler;
        private ServiceProvider _serviceProvider;
        private ILogger<SimpleCameraScheduler> _logger;
        private MockCameraFactory _mockCameraFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _logger = Substitute.For<ILogger<SimpleCameraScheduler>>();
            _mockCameraFactory = new MockCameraFactory();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IUnitOfWork>(_ => UnitOfWork);
            serviceCollection.AddSingleton<ICameraFactory>(_ => _mockCameraFactory);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _scheduler = new SimpleCameraScheduler(_serviceProvider, _logger);
        }

        [TearDown]
        public override void TearDown()
        {
            _scheduler?.Dispose();
            _serviceProvider?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task ExecuteDayNightModeAsync_WithValidParameters_ExecutesCommand()
        {
            // Arrange
            var sunriseSunset = SunriseSunset.Sunrise;
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _scheduler.ExecuteDayNightModeAsync(camera.Id, sunriseSunset);

            // Assert
            await _mockCameraFactory.MockCamera.Received(1).TriggerDayNightModeAsync(
                sunriseSunset,
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ResetAllCamerasAsync_WithEnabledCameras_SendsCorrectCommands()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;
            agent.TimeZoneOffset = -5;

            var camera1 = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera1.UpdateDayNightModeEnabled = true;
            camera1.Latitude = 40.7128;
            camera1.Longitude = -74.0060;

            var camera2 = TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua);
            camera2.UpdateDayNightModeEnabled = true;
            camera2.Latitude = 40.7128;
            camera2.Longitude = -74.0060;

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _scheduler.ResetAllCamerasAsync();

            // Assert
            await _mockCameraFactory.MockCamera.Received(2).TriggerDayNightModeAsync(
                Arg.Any<SunriseSunset>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ResetAllCamerasAsync_WithDisabledCameras_DoesNotSendCommands()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;

            var camera1 = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera1.UpdateDayNightModeEnabled = false;

            var camera2 = TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua);
            camera2.UpdateDayNightModeEnabled = false;

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _scheduler.ResetAllCamerasAsync();

            // Assert
            await _mockCameraFactory.MockCamera.DidNotReceive().TriggerDayNightModeAsync(
                Arg.Any<SunriseSunset>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ScheduleCameraAsync_WithValidParameters_SchedulesCamera()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;
            agent.TimeZoneOffset = -5;

            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;
            camera.Latitude = 40.7128;
            camera.Longitude = -74.0060;
            camera.TimezoneOffset = -4;

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _scheduler.ScheduleCameraAsync(camera.Id);

            // Assert
            var nextExecutionTime = _scheduler.GetNextScheduledExecutionTime(camera.Id);
            nextExecutionTime.Should().NotBeNull();
        }

        [Test]
        public async Task ScheduleCameraAsync_WithCameraSpecificSettings_UsesCameraSettings()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;

            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;
            camera.Latitude = 37.7749;  // San Francisco coordinates - different from agent
            camera.Longitude = -122.4194;
            camera.SunriseOffset = 30;
            camera.SunsetOffset = 60;

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _scheduler.ScheduleCameraAsync(camera.Id);

            // Assert
            var nextExecutionTime = _scheduler.GetNextScheduledExecutionTime(camera.Id);
            nextExecutionTime.Should().NotBeNull();
        }

        [Test]
        public async Task RemoveCameraScheduleAsync_WithScheduledCamera_RemovesSchedule()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;

            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;
            camera.Latitude = 40.7128;
            camera.Longitude = -74.0060;

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            await _scheduler.ScheduleCameraAsync(camera.Id);
            var nextExecutionTimeBefore = _scheduler.GetNextScheduledExecutionTime(camera.Id);

            // Act
            await _scheduler.RemoveCameraScheduleAsync(camera.Id);

            // Assert
            var nextExecutionTimeAfter = _scheduler.GetNextScheduledExecutionTime(camera.Id);
            nextExecutionTimeBefore.Should().NotBeNull();
            nextExecutionTimeAfter.Should().BeNull();
        }

        [Test]
        public async Task StartAsync_CallsResetAndScheduling()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;

            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;
            camera.Latitude = 40.7128;
            camera.Longitude = -74.0060;

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _scheduler.StartAsync();

            // Assert
            await _mockCameraFactory.MockCamera.Received(1).TriggerDayNightModeAsync(
                Arg.Any<SunriseSunset>(),
                Arg.Any<CancellationToken>());

            var nextExecutionTime = _scheduler.GetNextScheduledExecutionTime(camera.Id);
            nextExecutionTime.Should().NotBeNull();
        }

        [Test]
        public async Task GetAllScheduledJobs_WithScheduledCameras_ReturnsJobs()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;

            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;
            camera.Latitude = 40.7128;
            camera.Longitude = -74.0060;

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            await _scheduler.ScheduleCameraAsync(camera.Id);

            // Act
            var jobs = _scheduler.GetAllScheduledJobs();

            // Assert
            jobs.Should().HaveCount(1);
            jobs[0].CameraId.Should().Be(camera.Id);
            jobs[0].JobType.Should().Be(ScheduledJobType.SunriseSunset);
        }

        [Test]
        public async Task ScheduleOverlayAsync_WithValidRequest_SetsTextAndSchedulesClear()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateOverlayEnabled = true;

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var request = new CameraUpdateRequest
            {
                Id = camera.Id,
                IsTest = true,
                LicensePlate = "TEST123",
                AlertDescription = "Test Alert"
            };

            // Act
            await _scheduler.ScheduleOverlayAsync(request);

            // Assert
            await _mockCameraFactory.MockCamera.Received(1).SetCameraTextAsync(
                request,
                Arg.Any<CancellationToken>());

            var jobs = _scheduler.GetAllScheduledJobs();
            var overlayJob = jobs.FirstOrDefault(j => j.JobType == ScheduledJobType.ClearOverlay);
            overlayJob.Should().NotBeNull();
            overlayJob.CameraId.Should().Be(camera.Id);
        }
    }
} 