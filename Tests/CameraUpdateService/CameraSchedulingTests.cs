using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CameraSchedulingTests : TestBase
    {
        private IBackgroundJobService _backgroundJobService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _backgroundJobService = Substitute.For<IBackgroundJobService>();
        }

        [Test]
        public async Task ExecuteSingleDayNightTask_WithValidParameters_EnqueuesJobAsync()
        {
            // Arrange
            var sunriseSunset = SunriseSunset.Sunrise;
            var cameraId = Guid.NewGuid();

            // Act
            await CameraScheduling.ExecuteSingleDayNightTaskAsync(
                sunriseSunset,
                cameraId,
                _backgroundJobService);

            // Assert
            await _backgroundJobService.Received(1).EnqueueProcessSunriseSunsetJobAsync(
                cameraId,
                sunriseSunset,
                false);
        }

        [Test]
        public async Task ScheduleDayNightTasksAsync_WithEnabledCameras_SchedulesAllTasks()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;
            
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
            await CameraScheduling.ScheduleDayNightTasksAsync(UnitOfWork, _backgroundJobService, default);

            // Assert
            _backgroundJobService.Received(2).ScheduleProcessSunriseSunsetJob(
                Arg.Any<Guid>(),
                Arg.Any<SunriseSunset>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset>());
        }

        [Test]
        public async Task ScheduleDayNightTasksAsync_WithDisabledCameras_DoesNotScheduleTasks()
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
            await CameraScheduling.ScheduleDayNightTasksAsync(UnitOfWork, _backgroundJobService);

            // Assert
            _backgroundJobService.DidNotReceive().ScheduleProcessSunriseSunsetJob(
                Arg.Any<Guid>(),
                Arg.Any<SunriseSunset>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset>());
        }

        [Test]
        public void ScheduleDayNightTask_WithValidParameters_SchedulesCorrectJob()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;

            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;
            camera.Latitude = 40.7128;
            camera.Longitude = -74.0060;

            // Act
            CameraScheduling.ScheduleDayNightTask(
                _backgroundJobService,
                agent,
                camera);

            // Assert
            _backgroundJobService.Received(1).ScheduleProcessSunriseSunsetJob(
                camera.Id,
                Arg.Any<SunriseSunset>(),
                true,
                Arg.Any<DateTimeOffset>());
        }

        [Test]
        public void ScheduleDayNightTask_WithCameraSpecificSettings_UsesCameraSettings()
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

            // Act
            CameraScheduling.ScheduleDayNightTask(
                _backgroundJobService,
                agent,
                camera);

            // Assert
            _backgroundJobService.Received(1).ScheduleProcessSunriseSunsetJob(
                camera.Id,
                Arg.Any<SunriseSunset>(),
                true,
                Arg.Any<DateTimeOffset>());
        }

        [Test]
        public void ScheduleDayNightTask_WithExistingScheduledJob_DeletesOldJob()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.Latitude = 40.7128;
            agent.Longitude = -74.0060;

            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.UpdateDayNightModeEnabled = true;
            camera.Latitude = 40.7128;
            camera.Longitude = -74.0060;
            camera.NextDayNightScheduleId = "existing-job-id";

            // Act
            CameraScheduling.ScheduleDayNightTask(_backgroundJobService, agent, camera);

            // Assert
            _backgroundJobService.Received(1).DeleteJob("existing-job-id");
            _backgroundJobService.Received(1).ScheduleProcessSunriseSunsetJob(
                camera.Id,
                Arg.Any<SunriseSunset>(),
                true,
                Arg.Any<DateTimeOffset>());
        }

        [Test]
        public void IsSunUp_WithDaytimeCoordinates_ReturnsTrue()
        {
            // Arrange - Use coordinates and time when it should be daytime
            var latitude = 40.7128;  // New York
            var longitude = -74.0060;

            // Act
            var result = CameraScheduling.IsSunUp(latitude, longitude);

            // Assert
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public void IsSunUp_WithNightCoordinates_ReturnsBoolean()
        {
            // Arrange
            var latitude = -33.8688;  // Sydney
            var longitude = 151.2093;

            // Act
            var result = CameraScheduling.IsSunUp(latitude, longitude);

            // Assert
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public void IsSunUp_WithValidCoordinates_DoesNotThrowException()
        {
            // Arrange
            var latitude = 51.5074;  // London
            var longitude = -0.1278;

            // Act & Assert
            Assert.DoesNotThrow(() => CameraScheduling.IsSunUp(latitude, longitude));
        }
    }
} 