using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.SetZoomAndFocus;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraNightMode;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraOverlay;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCamera;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCameraMask;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System.Text.Json;
using Tests.TestHelpers;

namespace Tests.Features.Cameras.Commands
{
    [TestFixture]
    public class CameraCommandHandlersTests : TestBase
    {
        private ICameraUpdateService _cameraUpdateService;
        private IWebsocketClientOrganizer _websocketClientOrganizer;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _cameraUpdateService = Substitute.For<ICameraUpdateService>();
            _websocketClientOrganizer = Substitute.For<IWebsocketClientOrganizer>();
        }

        [Test]
        public async Task UpsertCameraCommandHandler_NewCamera_CreatesNewCamera()
        {
            // Arrange
            var handler = new UpsertCameraCommandHandler(UnitOfWork, _cameraUpdateService);
            var camera = TestDataFactory.CreateTestCameraUpdateServiceCamera("New Camera", 123);
            var command = new UpsertCameraCommand(camera);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var savedCamera = await UnitOfWork.Cameras.FirstOrDefaultAsync(c => c.Id == camera.Id);
            savedCamera.Should().NotBeNull();
            savedCamera.OpenAlprName.Should().Be("New Camera");
            savedCamera.OpenAlprCameraId.Should().Be(123);
        }

        [Test]
        public async Task UpsertCameraCommandHandler_ExistingCamera_UpdatesCamera()
        {
            // Arrange
            var handler = new UpsertCameraCommandHandler(UnitOfWork, _cameraUpdateService);
            var existingCamera = TestDataFactory.CreateTestCamera("Old Camera", 456);
            await UnitOfWork.Cameras.AddAsync(existingCamera);
            await UnitOfWork.SaveChangesAsync();

            var updatedCamera = TestDataFactory.CreateTestCameraUpdateServiceCamera("Updated Camera", 789);
            updatedCamera.Id = existingCamera.Id;
            var command = new UpsertCameraCommand(updatedCamera);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var savedCamera = await UnitOfWork.Cameras.FirstOrDefaultAsync(c => c.Id == existingCamera.Id);
            savedCamera.Should().NotBeNull();
            savedCamera.OpenAlprName.Should().Be("Updated Camera");
            savedCamera.OpenAlprCameraId.Should().Be(789);
        }

        [Test]
        public async Task UpsertCameraCommandHandler_CameraWithDayNightEnabled_SchedulesDayNightTask()
        {
            // Arrange
            var handler = new UpsertCameraCommandHandler(UnitOfWork, _cameraUpdateService);
            var camera = TestDataFactory.CreateTestCameraUpdateServiceCamera("Test Camera", 123);
            camera.DayNightModeEnabled = true;
            var command = new UpsertCameraCommand(camera);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).ScheduleDayNightTaskAsync();
        }

        [Test]
        public async Task UpsertCameraCommandHandler_CameraWithDayNightDisabled_DeletesSunriseSunset()
        {
            // Arrange
            var handler = new UpsertCameraCommandHandler(UnitOfWork, _cameraUpdateService);
            var camera = TestDataFactory.CreateTestCameraUpdateServiceCamera("Test Camera", 123);
            camera.DayNightModeEnabled = false;
            var command = new UpsertCameraCommand(camera);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).DeleteSunriseSunsetAsync(camera.Id);
        }

        [Test]
        public async Task DeleteCameraCommandHandler_ExistingCamera_DeletesCamera()
        {
            // Arrange
            var handler = new DeleteCameraCommandHandler(UnitOfWork, _cameraUpdateService);
            var camera = TestDataFactory.CreateTestCamera("Test Camera", 123);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteCameraCommand(camera.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var deletedCamera = await UnitOfWork.Cameras.FirstOrDefaultAsync(c => c.Id == camera.Id);
            deletedCamera.Should().BeNull();
            await _cameraUpdateService.Received(1).DeleteSunriseSunsetAsync(camera.Id);
        }

        [Test]
        public async Task DeleteCameraCommandHandler_NonExistentCamera_DoesNothing()
        {
            // Arrange
            var handler = new DeleteCameraCommandHandler(UnitOfWork, _cameraUpdateService);
            var nonExistentId = Guid.NewGuid();
            var command = new DeleteCameraCommand(nonExistentId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _cameraUpdateService.DidNotReceive().DeleteSunriseSunsetAsync(Arg.Any<Guid>());
        }

        [Test]
        public async Task UpsertCameraMaskCommandHandler_ExistingCameraWithNewMask_CreatesMask()
        {
            // Arrange
            var handler = new UpsertCameraMaskCommandHandler(UnitOfWork, _websocketClientOrganizer);
            var camera = TestDataFactory.CreateTestCamera("Test Camera", 123);
            var agent = TestDataFactory.CreateTestAgent();
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var coordinates = new List<MaskCoordinate>
            {
                new MaskCoordinate { X = 100, Y = 200 },
                new MaskCoordinate { X = 300, Y = 400 }
            };
            var cameraMask = new OpenAlprWebhookProcessor.Features.Cameras.CameraMask
            {
                CameraId = camera.Id,
                Coordinates = coordinates,
                ImageMask = "test-mask-data"
            };
            var command = new UpsertCameraMaskCommand(cameraMask);

            _websocketClientOrganizer.UpsertCameraMaskAsync(
                agent.Uid, 
                cameraMask.ImageMask, 
                camera.OpenAlprName, 
                Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            var savedCamera = await UnitOfWork.Cameras.GetQueryable()
                .Include(c => c.Mask)
                .FirstOrDefaultAsync(c => c.Id == camera.Id);
            savedCamera.Mask.Should().NotBeNull();
            savedCamera.Mask.Coordinates.Should().Be(JsonSerializer.Serialize(coordinates));
        }

        [Test]
        public async Task UpsertCameraMaskCommandHandler_ExistingCameraWithExistingMask_UpdatesMask()
        {
            // Arrange
            var handler = new UpsertCameraMaskCommandHandler(UnitOfWork, _websocketClientOrganizer);
            var camera = TestDataFactory.CreateTestCamera("Test Camera", 123);
            var agent = TestDataFactory.CreateTestAgent();
            
            // Create existing mask
            var existingMask = new OpenAlprWebhookProcessor.Features.Cameras.CameraMask
            {
                CameraId = camera.Id,
                Coordinates = new List<MaskCoordinate> { new MaskCoordinate { X = 50, Y = 60 } }
            };
            camera.Mask = new OpenAlprWebhookProcessor.Data.CameraMask
            {
                CameraId = camera.Id,
                Coordinates = JsonSerializer.Serialize(existingMask.Coordinates)
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var newCoordinates = new List<MaskCoordinate>
            {
                new MaskCoordinate { X = 100, Y = 200 },
                new MaskCoordinate { X = 300, Y = 400 }
            };
            var updatedCameraMask = new OpenAlprWebhookProcessor.Features.Cameras.CameraMask
            {
                CameraId = camera.Id,
                Coordinates = newCoordinates,
                ImageMask = "updated-mask-data"
            };
            var command = new UpsertCameraMaskCommand(updatedCameraMask);

            _websocketClientOrganizer.UpsertCameraMaskAsync(
                agent.Uid, 
                updatedCameraMask.ImageMask, 
                camera.OpenAlprName, 
                Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            var savedCamera = await UnitOfWork.Cameras.GetQueryable()
                .Include(c => c.Mask)
                .FirstOrDefaultAsync(c => c.Id == camera.Id);
            savedCamera.Mask.Should().NotBeNull();
            savedCamera.Mask.Coordinates.Should().Be(JsonSerializer.Serialize(newCoordinates));
        }

        [Test]
        public async Task UpsertCameraMaskCommandHandler_EmptyCoordinates_RemovesMask()
        {
            // Arrange
            var handler = new UpsertCameraMaskCommandHandler(UnitOfWork, _websocketClientOrganizer);
            var camera = TestDataFactory.CreateTestCamera("Test Camera", 123);
            var agent = TestDataFactory.CreateTestAgent();
            
            // Create existing mask
            camera.Mask = new OpenAlprWebhookProcessor.Data.CameraMask
            {
                CameraId = camera.Id,
                Coordinates = JsonSerializer.Serialize(new List<MaskCoordinate> { new MaskCoordinate { X = 50, Y = 60 } })
            };

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var emptyMask = new OpenAlprWebhookProcessor.Features.Cameras.CameraMask
            {
                CameraId = camera.Id,
                Coordinates = new List<MaskCoordinate>(),
                ImageMask = ""
            };
            var command = new UpsertCameraMaskCommand(emptyMask);

            _websocketClientOrganizer.UpsertCameraMaskAsync(
                agent.Uid, 
                emptyMask.ImageMask, 
                camera.OpenAlprName, 
                Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            var savedCamera = await UnitOfWork.Cameras.GetQueryable()
                .Include(c => c.Mask)
                .FirstOrDefaultAsync(c => c.Id == camera.Id);
            savedCamera.Mask.Should().BeNull();
        }

        [Test]
        public async Task UpsertCameraMaskCommandHandler_NonExistentCamera_ReturnsFalse()
        {
            // Arrange
            var handler = new UpsertCameraMaskCommandHandler(UnitOfWork, _websocketClientOrganizer);
            var nonExistentId = Guid.NewGuid();
            var cameraMask = new OpenAlprWebhookProcessor.Features.Cameras.CameraMask
            {
                CameraId = nonExistentId,
                Coordinates = new List<MaskCoordinate> { new MaskCoordinate { X = 100, Y = 200 } },
                ImageMask = "test-mask-data"
            };
            var command = new UpsertCameraMaskCommand(cameraMask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task TestCameraDayModeCommandHandler_ValidCommand_EnqueuesDayMode()
        {
            // Arrange
            var handler = new TestCameraDayModeCommandHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            var command = new TestCameraDayModeCommand(cameraId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).EnqueueDayNightAsync(cameraId, SunriseSunset.Sunrise);
        }

        [Test]
        public async Task TestCameraNightModeCommandHandler_ValidCommand_EnqueuesNightMode()
        {
            // Arrange
            var handler = new TestCameraNightModeCommandHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            var command = new TestCameraNightModeCommand(cameraId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).EnqueueDayNightAsync(cameraId, SunriseSunset.Sunset);
        }

        [Test]
        public async Task TestCameraOverlayCommandHandler_ValidCommand_SchedulesOverlayRequest()
        {
            // Arrange
            var handler = new TestCameraOverlayCommandHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            var command = new TestCameraOverlayCommand(cameraId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).ScheduleOverlayRequestAsync(
                Arg.Is<CameraUpdateRequest>(r =>
                    r.Id == cameraId &&
                    r.IsTest == true &&
                    r.LicensePlate == "test" &&
                    r.AlertDescription == "test"));
        }

        [Test]
        public async Task SetZoomAndFocusCommandHandler_ValidCommand_CallsSetZoomAndFocus()
        {
            // Arrange
            var handler = new SetZoomAndFocusCommandHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            var zoomFocus = new ZoomFocus { Zoom = 1.5m, Focus = 2.0m };
            var command = new SetZoomAndFocusCommand(cameraId, zoomFocus);
            var cancellationToken = CancellationToken.None;

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            await _cameraUpdateService.Received(1).SetZoomAndFocusAsync(cameraId, zoomFocus, cancellationToken);
        }

        [Test]
        public async Task TriggerAutofocusCommandHandler_ValidCommand_CallsTriggerAutofocus()
        {
            // Arrange
            var handler = new TriggerAutofocusCommandHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            var command = new TriggerAutofocusCommand(cameraId);
            var cancellationToken = CancellationToken.None;

            _cameraUpdateService.TriggerAutofocusAsync(cameraId, cancellationToken)
                .Returns(true);

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();
            await _cameraUpdateService.Received(1).TriggerAutofocusAsync(cameraId, cancellationToken);
        }

        [Test]
        public async Task TriggerAutofocusCommandHandler_ServiceReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var handler = new TriggerAutofocusCommandHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            var command = new TriggerAutofocusCommand(cameraId);
            var cancellationToken = CancellationToken.None;

            _cameraUpdateService.TriggerAutofocusAsync(cameraId, cancellationToken)
                .Returns(false);

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeFalse();
        }
    }
} 