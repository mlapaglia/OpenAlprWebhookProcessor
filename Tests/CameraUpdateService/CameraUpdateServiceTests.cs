using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using Tests.TestHelpers;
using NSubstitute.ExceptionExtensions;
using DataCamera = OpenAlprWebhookProcessor.Data.Camera;

namespace Tests.CameraUpdateService
{
    [TestFixture]
    public class CameraUpdateServiceTests : TestBase
    {
        private OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService _cameraUpdateService;
        private ILogger<OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService> _logger;
        private IBackgroundJobService _backgroundJobService;
        private ICameraFactory _cameraFactory;
        private ICamera _mockCamera;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _logger = Substitute.For<ILogger<OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService>>();
            _backgroundJobService = Substitute.For<IBackgroundJobService>();
            _cameraFactory = Substitute.For<ICameraFactory>();
            _mockCamera = Substitute.For<ICamera>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(UnitOfWork);
            serviceCollection.AddSingleton(_cameraFactory);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _cameraFactory.Create(Arg.Any<CameraManufacturer>(), Arg.Any<DataCamera>())
                .Returns(_mockCamera);

            _cameraUpdateService = new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(
                _serviceProvider,
                _logger,
                _backgroundJobService);
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(
                null, _logger, _backgroundJobService));
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(
                _serviceProvider, null, _backgroundJobService));
        }

        [Test]
        public void Constructor_WithNullBackgroundJobService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(
                _serviceProvider, _logger, null));
        }

        [Test]
        public async Task DeleteSunriseSunsetAsync_WithExistingCamera_DeletesScheduledJob()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.NextDayNightScheduleId = "scheduled-job-id";
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _cameraUpdateService.DeleteSunriseSunsetAsync(camera.Id);

            // Assert
            _backgroundJobService.Received(1).DeleteJob("scheduled-job-id");
        }

        [Test]
        public void DeleteSunriseSunsetAsync_WithNonExistentCamera_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentCameraId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _cameraUpdateService.DeleteSunriseSunsetAsync(nonExistentCameraId));

            Assert.That(exception.Message, Contains.Substring("Camera not found"));
        }

        [Test]
        public async Task DeleteSunriseSunsetAsync_WithNoScheduledJob_DoesNotDeleteJob()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.NextDayNightScheduleId = null;
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _cameraUpdateService.DeleteSunriseSunsetAsync(camera.Id);

            // Assert
            _backgroundJobService.DidNotReceive().DeleteJob(Arg.Any<string>());
        }

        [Test]
        public async Task ProcessSunriseSunsetJobAsync_WithValidCamera_ProcessesSuccessfully()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            
            var agent = TestDataFactory.CreateTestAgent();
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _cameraUpdateService.ProcessSunriseSunsetJobAsync(camera.Id, SunriseSunset.Sunrise, false);

            // Assert
            await _mockCamera.Received(1).SetCameraTextAsync(Arg.Any<CameraUpdateRequest>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void ProcessSunriseSunsetJobAsync_WithNonExistentCamera_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentCameraId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _cameraUpdateService.ProcessSunriseSunsetJobAsync(nonExistentCameraId, SunriseSunset.Sunrise, false));

            Assert.That(exception.Message, Is.EqualTo("camera not found"));
        }

        [Test]
        public async Task ProcessSunriseSunsetJobAsync_WithCameraException_LogsError()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            
            var agent = TestDataFactory.CreateTestAgent();
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _mockCamera.SetCameraTextAsync(Arg.Any<CameraUpdateRequest>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Camera error"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _cameraUpdateService.ProcessSunriseSunsetJobAsync(camera.Id, SunriseSunset.Sunrise, false));
            
            exception.Message.Should().Be("Camera error");
        }

        [Test]
        public async Task ProcessJobAsync_WithValidRequest_ProcessesSuccessfully()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var request = new CameraUpdateRequest
            {
                Id = camera.Id,
                LicensePlate = "ABC123",
                VehicleDescription = "Red Car",
                OpenAlprProcessingTimeMs = 150,
                ProcessedPlateConfidence = 95.5,
                IsTest = false
            };

            // Act
            await _cameraUpdateService.ProcessJobAsync(request);

            // Assert
            await _mockCamera.Received(1).SetCameraTextAsync(request, Arg.Any<CancellationToken>());
        }

        [Test]
        public void ProcessJobAsync_WithNonExistentCamera_ThrowsArgumentException()
        {
            // Arrange
            var request = new CameraUpdateRequest
            {
                Id = Guid.NewGuid(),
                LicensePlate = "ABC123"
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _cameraUpdateService.ProcessJobAsync(request));

            Assert.That(exception.Message, Contains.Substring("unknown camera Id"));
        }

        [Test]
        public async Task ProcessJobAsync_WithTestRequest_DoesNotUpdateStatistics()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var initialPlatesSeen = camera.PlatesSeen;

            var request = new CameraUpdateRequest
            {
                Id = camera.Id,
                LicensePlate = "ABC123",
                IsTest = true
            };

            // Act
            await _cameraUpdateService.ProcessJobAsync(request);

            // Assert
            Assert.That(camera.PlatesSeen, Is.EqualTo(initialPlatesSeen));
        }

        [Test]
        public async Task ClearExpiredOverlayAsync_WithValidCamera_ClearsOverlay()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.NextClearOverlayScheduleId = "clear-job-id";
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _cameraUpdateService.ClearExpiredOverlayAsync(camera.Id);

            // Assert
            await _mockCamera.Received(1).ClearCameraTextAsync(Arg.Any<CancellationToken>());
            Assert.That(camera.NextClearOverlayScheduleId, Is.Empty);
        }

        [Test]
        public async Task ClearExpiredOverlayAsync_WithNonExistentCamera_LogsError()
        {
            // Arrange
            var nonExistentCameraId = Guid.NewGuid();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _cameraUpdateService.ClearExpiredOverlayAsync(nonExistentCameraId));
        }

        [Test]
        public async Task ClearExpiredOverlayAsync_WithCameraException_LogsError()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _mockCamera.ClearCameraTextAsync(Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Camera error"));

            // Act
            await _cameraUpdateService.ClearExpiredOverlayAsync(camera.Id);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Test]
        public async Task GetZoomAndFocusAsync_WithValidCamera_ReturnsZoomFocus()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var expectedZoomFocus = new ZoomFocus { Zoom = 2.5m, Focus = 3.0m };
            _mockCamera.GetZoomAndFocusAsync(Arg.Any<CancellationToken>())
                .Returns(expectedZoomFocus);

            // Act
            var result = await _cameraUpdateService.GetZoomAndFocusAsync(camera.Id, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(expectedZoomFocus));
        }

        [Test]
        public void GetZoomAndFocusAsync_WithNonExistentCamera_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentCameraId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _cameraUpdateService.GetZoomAndFocusAsync(nonExistentCameraId, CancellationToken.None));

            Assert.That(exception.Message, Contains.Substring("Camera not found"));
        }

        [Test]
        public async Task SetZoomAndFocusAsync_WithValidCamera_SetsZoomFocus()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var zoomFocus = new ZoomFocus { Zoom = 2.5m, Focus = 3.0m };

            // Act
            await _cameraUpdateService.SetZoomAndFocusAsync(camera.Id, zoomFocus, CancellationToken.None);

            // Assert
            await _mockCamera.Received(1).SetZoomAndFocusAsync(zoomFocus, CancellationToken.None);
        }

        [Test]
        public void SetZoomAndFocusAsync_WithNonExistentCamera_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentCameraId = Guid.NewGuid();
            var zoomFocus = new ZoomFocus { Zoom = 2.5m, Focus = 3.0m };

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _cameraUpdateService.SetZoomAndFocusAsync(nonExistentCameraId, zoomFocus, CancellationToken.None));

            Assert.That(exception.Message, Contains.Substring("Camera not found"));
        }

        [Test]
        public async Task TriggerAutofocusAsync_WithValidCamera_TriggersAutofocus()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _mockCamera.TriggerAutoFocusAsync(Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await _cameraUpdateService.TriggerAutofocusAsync(camera.Id, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            await _mockCamera.Received(1).TriggerAutoFocusAsync(CancellationToken.None);
        }

        [Test]
        public void TriggerAutofocusAsync_WithNonExistentCamera_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentCameraId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _cameraUpdateService.TriggerAutofocusAsync(nonExistentCameraId, CancellationToken.None));

            Assert.That(exception.Message, Contains.Substring("Camera not found"));
        }

        [Test]
        public async Task ForceClearOverlaysAsync_WithMultipleCameras_ClearsAllOverlays()
        {
            // Arrange
            var camera1 = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            var camera2 = TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua);
            
            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _cameraUpdateService.ForceClearOverlaysAsync();

            // Assert
            await _mockCamera.Received(2).ClearCameraTextAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ForceClearOverlaysAsync_WithCameraException_LogsErrorAndContinues()
        {
            // Arrange
            var camera1 = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            var camera2 = TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua);
            
            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            _mockCamera.ClearCameraTextAsync(Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Camera error"));

            // Act
            await _cameraUpdateService.ForceClearOverlaysAsync();

            // Assert
            _logger.Received(2).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Test]
        public void ScheduleOverlayRequest_WithValidRequest_SchedulesJob()
        {
            // Arrange
            var request = new CameraUpdateRequest
            {
                Id = Guid.NewGuid(),
                LicensePlate = "ABC123"
            };

            // Act
            _cameraUpdateService.ScheduleOverlayRequest(request);

            // Assert
            _backgroundJobService.Received(1).EnqueueProcessJob(request);
        }

        [Test]
        public async Task StartAsync_ReturnsCompletedTask()
        {
            // Act
            await _cameraUpdateService.StartAsync(CancellationToken.None);

            // Assert
            Assert.Pass(); // StartAsync should complete without throwing
        }

        [Test]
        public async Task StopAsync_CallsForceClearOverlaysAsync()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _cameraUpdateService.StopAsync(CancellationToken.None);

            // Assert
            await _mockCamera.Received().ClearCameraTextAsync(Arg.Any<CancellationToken>());
        }

        [TearDown]
        public override void TearDown()
        {
            (_serviceProvider as IDisposable)?.Dispose();
            base.TearDown();
        }
    }
} 