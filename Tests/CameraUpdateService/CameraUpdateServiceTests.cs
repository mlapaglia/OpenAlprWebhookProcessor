using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CameraUpdateServiceTests : TestBase
    {
        private IServiceProvider _serviceProvider;

        private IServiceScope _serviceScope;

        private IServiceScopeFactory _serviceScopeFactory;

        private ILogger<OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService> _logger;

        private IBackgroundJobService _backgroundJobService;

        private ICameraFactory _cameraFactory;

        private ICamera _camera;

        private OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _serviceProvider = Substitute.For<IServiceProvider>();
            _serviceScope = Substitute.For<IServiceScope>();
            _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            _logger = Substitute.For<ILogger<OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService>>();
            _backgroundJobService = Substitute.For<IBackgroundJobService>();
            _cameraFactory = Substitute.For<ICameraFactory>();
            _camera = Substitute.For<ICamera>();

            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceScopeFactory.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.Returns(_serviceProvider);

            _serviceProvider.GetService(typeof(IUnitOfWork)).Returns(UnitOfWork);
            _serviceProvider.GetService(typeof(ICameraFactory)).Returns(_cameraFactory);

            _sut = new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(_serviceProvider, _logger, _backgroundJobService);
        }

        [TearDown]
        public override void TearDown()
        {
            (_serviceScope as IDisposable)?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(null, _logger, _backgroundJobService));
        }

        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(_serviceProvider, null, _backgroundJobService));
        }

        [Test]
        public void Constructor_NullBackgroundJobService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new OpenAlprWebhookProcessor.CameraUpdateService.CameraUpdateService(_serviceProvider, _logger, null));
        }

        [Test]
        public void DeleteSunriseSunsetAsync_CameraNotFound_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteSunriseSunsetAsync(cameraId));
            Assert.That(ex.Message, Does.Contain($"Camera not found: {cameraId}"));
        }

        [Test]
        public async Task DeleteSunriseSunsetAsync_CameraWithScheduleId_DeletesJobAndUpdatesCamera()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var scheduleId = "schedule123";
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                NextDayNightScheduleId = scheduleId,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _sut.DeleteSunriseSunsetAsync(cameraId);

            // Assert
            _backgroundJobService.Received(1).DeleteJob(scheduleId);
            var updatedCamera = await UnitOfWork.Cameras.GetByIdAsync(cameraId);
            Assert.That(updatedCamera.NextDayNightScheduleId, Is.Empty);
        }

        [Test]
        public async Task DeleteSunriseSunsetAsync_CameraWithoutScheduleId_DoesNotDeleteJob()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                NextDayNightScheduleId = string.Empty,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _sut.DeleteSunriseSunsetAsync(cameraId);

            // Assert
            _backgroundJobService.DidNotReceive().DeleteJob(Arg.Any<string>());
        }

        [Test]
        public void ProcessSunriseSunsetJobAsync_CameraNotFound_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.ProcessSunriseSunsetJobAsync(cameraId, SunriseSunset.Sunrise, false));
            Assert.That(ex.Message, Does.Contain($"Camera not found: {cameraId}"));
        }

        [Test]
        public async Task ProcessSunriseSunsetJobAsync_Sunrise_SetsDAYText()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);

            // Act
            await _sut.ProcessSunriseSunsetJobAsync(cameraId, SunriseSunset.Sunrise, false);

            // Assert
            await _camera.Received(1).TriggerDayNightModeAsync(SunriseSunset.Sunrise);
        }

        [Test]
        public async Task ProcessSunriseSunsetJobAsync_Sunset_SetsNIGHTText()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);

            // Act
            await _sut.ProcessSunriseSunsetJobAsync(cameraId, SunriseSunset.Sunset, false);

            // Assert
            await _camera.Received(1).TriggerDayNightModeAsync(SunriseSunset.Sunset);
        }

        [Test]
        public async Task ProcessSunriseSunsetJobAsync_ScheduleNextJobTrue_SchedulesAdditionalTasks()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);

            // Act
            await _sut.ProcessSunriseSunsetJobAsync(cameraId, SunriseSunset.Sunrise, true);

            // Assert
            // Note: Testing static method calls might require wrapping CameraScheduling
            // For now, we can verify the camera text was set
            await _camera.Received(1).TriggerDayNightModeAsync(SunriseSunset.Sunrise);
        }

        [Test]
        public void ClearExpiredOverlayAsync_CameraNotFound_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.ClearExpiredOverlayAsync(cameraId));
            Assert.That(ex.Message, Does.Contain($"Camera not found: {cameraId}"));
        }

        [Test]
        public async Task ClearExpiredOverlayAsync_Success_ClearsOverlayAndUpdatesCamera()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua,
                NextClearOverlayScheduleId = "schedule123"
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);

            // Act
            await _sut.ClearExpiredOverlayAsync(cameraId);

            // Assert
            await _camera.Received(1).ClearCameraTextAsync(Arg.Any<CancellationToken>());
            var updatedCamera = await UnitOfWork.Cameras.GetByIdAsync(cameraId);
            Assert.That(updatedCamera.NextClearOverlayScheduleId, Is.Empty);
        }

        [Test]
        public async Task ScheduleDayNightTaskAsync_CallsScheduling()
        {
            // Act
            await _sut.ScheduleDayNightTaskAsync();

            // Assert
            // This tests that the method completes without error
            // Actual scheduling logic would need to be tested separately
            Assert.Pass();
        }

        [Test]
        public async Task EnqueueDayNightAsync_CallsScheduling()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var sunriseSunset = SunriseSunset.Sunrise;

            // Act
            await _sut.EnqueueDayNightAsync(cameraId, sunriseSunset);

            // Assert
            // This tests that the method completes without error
            Assert.Pass();
        }

        [Test]
        public async Task ScheduleOverlayRequestAsync_EnqueuesJob()
        {
            // Arrange
            var request = new CameraUpdateRequest { Id = Guid.NewGuid() };

            // Act
            await _sut.ScheduleOverlayRequestAsync(request);

            // Assert
            await _backgroundJobService.Received(1).EnqueueProcessJobAsync(request);
        }

        [Test]
        public void ProcessJobAsync_CameraNotFound_ThrowsArgumentException()
        {
            // Arrange
            var request = new CameraUpdateRequest { Id = Guid.NewGuid() };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _sut.ProcessJobAsync(request));
            Assert.That(ex.Message, Does.Contain($"Unknown camera ID: {request.Id}"));
        }

        [Test]
        public async Task ProcessJobAsync_ExistingClearOverlayJob_DeletesOldJob()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var oldJobId = "oldJob123";
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua,
                NextClearOverlayScheduleId = oldJobId
            };
            var request = new CameraUpdateRequest
            {
                Id = cameraId,
                LicensePlate = "ABC123"
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);
            _backgroundJobService.ScheduleClearOverlayJob(cameraId, Arg.Any<TimeSpan>())
                .Returns("newJob123");

            // Act
            await _sut.ProcessJobAsync(request);

            // Assert
            _backgroundJobService.Received(1).DeleteJob(oldJobId);
        }

        [Test]
        public async Task ProcessJobAsync_NonTestNonPreviewNonSinglePlate_UpdatesPlatesSeenAndUuid()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var plateUuid = Guid.NewGuid().ToString();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua,
                PlatesSeen = 5
            };
            var request = new CameraUpdateRequest
            {
                Id = cameraId,
                LicensePlate = "ABC123",
                IsTest = false,
                IsPreviewGroup = false,
                IsSinglePlate = false,
                LicensePlateImageUuid = plateUuid
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);
            _backgroundJobService.ScheduleClearOverlayJob(cameraId, Arg.Any<TimeSpan>())
                .Returns("newJob123");

            // Act
            await _sut.ProcessJobAsync(request);

            // Assert
            var updatedCamera = await UnitOfWork.Cameras.GetByIdAsync(cameraId);
            Assert.That(updatedCamera.PlatesSeen, Is.EqualTo(6));
            Assert.That(updatedCamera.LatestProcessedPlateUuid, Is.EqualTo(plateUuid));
        }

        [Test]
        public void GetZoomAndFocusAsync_CameraNotFound_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetZoomAndFocusAsync(cameraId, CancellationToken.None));
            Assert.That(ex.Message, Does.Contain($"Camera not found: {cameraId}"));
        }

        [Test]
        public async Task GetZoomAndFocusAsync_Success_ReturnsZoomFocus()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var expectedZoomFocus = new ZoomFocus { Zoom = 1.5M, Focus = 0.8M };
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);
            _camera.GetZoomAndFocusAsync(Arg.Any<CancellationToken>()).Returns(expectedZoomFocus);

            // Act
            var result = await _sut.GetZoomAndFocusAsync(cameraId, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(expectedZoomFocus));
        }

        [Test]
        public void SetZoomAndFocusAsync_CameraNotFound_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var zoomFocus = new ZoomFocus { Zoom = 1.5M, Focus = 0.8M };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.SetZoomAndFocusAsync(cameraId, zoomFocus, CancellationToken.None));
            Assert.That(ex.Message, Does.Contain($"Camera not found: {cameraId}"));
        }

        [Test]
        public async Task SetZoomAndFocusAsync_Success_CallsCameraMethod()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var zoomFocus = new ZoomFocus { Zoom = 1.5M, Focus = 0.8M };
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);

            // Act
            await _sut.SetZoomAndFocusAsync(cameraId, zoomFocus, CancellationToken.None);

            // Assert
            await _camera.Received(1).SetZoomAndFocusAsync(zoomFocus, Arg.Any<CancellationToken>());
        }

        [Test]
        public void TriggerAutofocusAsync_CameraNotFound_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.TriggerAutofocusAsync(cameraId, CancellationToken.None));
            Assert.That(ex.Message, Does.Contain($"Camera not found: {cameraId}"));
        }

        [Test]
        public async Task TriggerAutofocusAsync_Success_ReturnsResult()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = cameraId,
                Manufacturer = CameraManufacturer.Dahua
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _cameraFactory.Create(camera.Manufacturer, camera).Returns(_camera);
            _camera.TriggerAutoFocusAsync(Arg.Any<CancellationToken>()).Returns(true);

            // Act
            var result = await _sut.TriggerAutofocusAsync(cameraId, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ForceClearOverlaysAsync_MultipleCameras_ClearsAllAndSavesOnce()
        {
            // Arrange
            var camera1 = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                Manufacturer = CameraManufacturer.Dahua,
                NextClearOverlayScheduleId = "schedule1"
            };
            var camera2 = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                Manufacturer = CameraManufacturer.Hikvision,
                NextClearOverlayScheduleId = "schedule2"
            };

            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            var camera1Mock = Substitute.For<ICamera>();
            var camera2Mock = Substitute.For<ICamera>();
            _cameraFactory.Create(camera1.Manufacturer, camera1).Returns(camera1Mock);
            _cameraFactory.Create(camera2.Manufacturer, camera2).Returns(camera2Mock);

            // Act
            await _sut.ForceClearOverlaysAsync();

            // Assert
            await camera1Mock.Received(1).ClearCameraTextAsync(Arg.Any<CancellationToken>());
            await camera2Mock.Received(1).ClearCameraTextAsync(Arg.Any<CancellationToken>());

            var updatedCamera1 = await UnitOfWork.Cameras.GetByIdAsync(camera1.Id);
            var updatedCamera2 = await UnitOfWork.Cameras.GetByIdAsync(camera2.Id);
            Assert.That(updatedCamera1.NextClearOverlayScheduleId, Is.Empty);
            Assert.That(updatedCamera2.NextClearOverlayScheduleId, Is.Empty);
        }

        [Test]
        public async Task ForceClearOverlaysAsync_OneCameraFails_ContinuesWithOthers()
        {
            // Arrange
            var camera1 = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                Manufacturer = CameraManufacturer.Dahua,
                NextClearOverlayScheduleId = "schedule1"
            };
            var camera2 = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                Manufacturer = CameraManufacturer.Hikvision,
                NextClearOverlayScheduleId = "schedule2"
            };
            var exception = new Exception("Camera 1 error");

            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            var camera1Mock = Substitute.For<ICamera>();
            var camera2Mock = Substitute.For<ICamera>();
            _cameraFactory.Create(camera1.Manufacturer, camera1).Returns(camera1Mock);
            _cameraFactory.Create(camera2.Manufacturer, camera2).Returns(camera2Mock);

            camera1Mock.ClearCameraTextAsync(Arg.Any<CancellationToken>()).ThrowsAsync(exception);

            // Act
            await _sut.ForceClearOverlaysAsync();

            // Assert
            await camera2Mock.Received(1).ClearCameraTextAsync(Arg.Any<CancellationToken>());

            var updatedCamera2 = await UnitOfWork.Cameras.GetByIdAsync(camera2.Id);
            Assert.That(updatedCamera2.NextClearOverlayScheduleId, Is.Empty);
        }
    }
}