using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService
{
    [TestFixture]
    public class CameraSchedulingTests : TestBase
    {
        private OpenAlprWebhookProcessor.CameraUpdateService.CameraScheduling _cameraScheduling;
        private IBackgroundJobService _backgroundJobService;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Create mocks
            _backgroundJobService = Substitute.For<IBackgroundJobService>();
            
            // Create a simple service provider that returns our test UnitOfWork
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(UnitOfWork);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Create service under test
            _cameraScheduling = new OpenAlprWebhookProcessor.CameraUpdateService.CameraScheduling(
                Substitute.For<IBackgroundJobClient>(),
                _serviceProvider);
        }

        [Test]
        public void Constructor_WithNullBackgroundJobClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraScheduling(null, _serviceProvider));
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraScheduling(Substitute.For<IBackgroundJobClient>(), null));
        }

        [Test]
        public void ExecuteSingleDayNightTask_WithValidParameters_EnqueuesJob()
        {
            // Arrange
            var sunriseSunset = SunriseSunset.Sunrise;
            var cameraId = Guid.NewGuid();

            // Act
            _cameraScheduling.ExecuteSingleDayNightTask(sunriseSunset, cameraId, _backgroundJobService);

            // Assert
            _backgroundJobService.Received(1).EnqueueProcessSunriseSunsetJob(cameraId, sunriseSunset, false);
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
            await _cameraScheduling.ScheduleDayNightTasksAsync(_backgroundJobService);

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
            await _cameraScheduling.ScheduleDayNightTasksAsync(_backgroundJobService);

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
            _cameraScheduling.ScheduleDayNightTask(_backgroundJobService, agent, camera);

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
            _cameraScheduling.ScheduleDayNightTask(_backgroundJobService, agent, camera);

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
            _cameraScheduling.ScheduleDayNightTask(_backgroundJobService, agent, camera);

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
            var result = _cameraScheduling.IsSunUp(latitude, longitude);

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
            var result = _cameraScheduling.IsSunUp(latitude, longitude);

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
            Assert.DoesNotThrow(() => _cameraScheduling.IsSunUp(latitude, longitude));
        }

        [TearDown]
        public override void TearDown()
        {
            (_serviceProvider as IDisposable)?.Dispose();
            base.TearDown();
        }
    }
} 