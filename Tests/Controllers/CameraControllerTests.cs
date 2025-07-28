using AwesomeAssertions;
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
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameras;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetPlateCaptures;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class CameraControllerTests : TestBase
    {
        private CameraController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new CameraController(Mediator);
        }

        [Test]
        public async Task GetCameras_ReturnsCorrectResult()
        {
            // Arrange
            var expectedCameras = new List<Camera>
            {
                TestDataFactory.CreateTestCameraUpdateServiceCamera("Camera 1", 1),
                TestDataFactory.CreateTestCameraUpdateServiceCamera("Camera 2", 2)
            };

            Mediator.Send(Arg.Any<GetCamerasQuery>())
                .Returns(expectedCameras);

            // Act
            var result = await _controller.GetCameras();

            // Assert
            result.Should().HaveCount(2);
            result[0].OpenAlprName.Should().Be("Camera 1");
            result[1].OpenAlprName.Should().Be("Camera 2");
        }

        [Test]
        public async Task GetCameras_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            Mediator.Send(Arg.Any<GetCamerasQuery>())
                .Returns(new List<Camera>());

            // Act
            var result = await _controller.GetCameras();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task UpsertCamera_ValidCamera_CallsCorrectCommand()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCameraUpdateServiceCamera("Test Camera", 123);

            // Act
            await _controller.UpsertCamera(camera);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertCameraCommand>(cmd => 
                    cmd.Camera.OpenAlprName == "Test Camera" && 
                    cmd.Camera.OpenAlprCameraId == 123));
        }

        [Test]
        public async Task UpsertCamera_NullCamera_CallsCommandWithNullCamera()
        {
            // Arrange
            Camera nullCamera = null;

            // Act
            await _controller.UpsertCamera(nullCamera);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertCameraCommand>(cmd => cmd.Camera == null));
        }

        [Test]
        public async Task DeleteCamera_ValidId_CallsCorrectCommand()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act
            await _controller.DeleteCamera(cameraId);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DeleteCameraCommand>(cmd => cmd.CameraId == cameraId));
        }

        [Test]
        public async Task DeleteCamera_EmptyGuid_CallsCommandWithEmptyGuid()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            await _controller.DeleteCamera(emptyGuid);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DeleteCameraCommand>(cmd => cmd.CameraId == emptyGuid));
        }

        [Test]
        public async Task TestOverlay_ValidId_ReturnsOk()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act
            var result = await _controller.TestOverlay(cameraId);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(
                Arg.Is<TestCameraOverlayCommand>(cmd => cmd.CameraId == cameraId));
        }

        [Test]
        public async Task TestNightMode_ValidId_ReturnsOk()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act
            var result = await _controller.TestNightMode(cameraId);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(
                Arg.Is<TestCameraNightModeCommand>(cmd => cmd.CameraId == cameraId));
        }

        [Test]
        public async Task TestDayMode_ValidId_ReturnsOk()
        {
            // Arrange
            var cameraId = Guid.NewGuid();

            // Act
            var result = await _controller.TestDayMode(cameraId);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(
                Arg.Is<TestCameraDayModeCommand>(cmd => cmd.CameraId == cameraId));
        }

        [Test]
        public async Task GetZoomAndFocus_ValidId_ReturnsCorrectResult()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var expectedZoomFocus = new ZoomFocus
            {
                Zoom = 1.5m,
                Focus = 2.0m
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetZoomAndFocusQuery>(), cancellationToken)
                .Returns(expectedZoomFocus);

            // Act
            var result = await _controller.GetZoomAndFocus(cameraId, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Zoom.Should().Be(1.5m);
            result.Focus.Should().Be(2.0m);
            
            await Mediator.Received(1).Send(
                Arg.Is<GetZoomAndFocusQuery>(q => q.CameraId == cameraId), 
                cancellationToken);
        }

        [Test]
        public async Task GetZoomAndFocus_ReturnsNull_ReturnsNull()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetZoomAndFocusQuery>(), cancellationToken)
                .Returns((ZoomFocus)null);

            // Act
            var result = await _controller.GetZoomAndFocus(cameraId, cancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task SetZoomAndFocus_ValidData_CallsCorrectCommand()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var zoomFocus = new ZoomFocus { Zoom = 1.5m, Focus = 2.0m };
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.SetZoomAndFocus(cameraId, zoomFocus, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<SetZoomAndFocusCommand>(cmd => 
                    cmd.CameraId == cameraId && 
                    cmd.ZoomAndFocus.Zoom == 1.5m && 
                    cmd.ZoomAndFocus.Focus == 2.0m), 
                cancellationToken);
        }

        [Test]
        public async Task SetZoomAndFocus_NullZoomFocus_CallsCommandWithNull()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            ZoomFocus nullZoomFocus = null;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.SetZoomAndFocus(cameraId, nullZoomFocus, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<SetZoomAndFocusCommand>(cmd => 
                    cmd.CameraId == cameraId && 
                    cmd.ZoomAndFocus == null), 
                cancellationToken);
        }

        [Test]
        public async Task TriggerAutofocus_ValidId_ReturnsResult()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<TriggerAutofocusCommand>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.TriggerAutofocus(cameraId, cancellationToken);

            // Assert
            result.Should().BeTrue();
            await Mediator.Received(1).Send(
                Arg.Is<TriggerAutofocusCommand>(cmd => cmd.CameraId == cameraId), 
                cancellationToken);
        }

        [Test]
        public async Task TriggerAutofocus_ReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<TriggerAutofocusCommand>(), cancellationToken)
                .Returns(false);

            // Act
            var result = await _controller.TriggerAutofocus(cameraId, cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task UpsertImageMask_ValidMask_ReturnsResult()
        {
            // Arrange
            var cameraMask = new CameraMask
            {
                CameraId = Guid.NewGuid(),
                Coordinates = new List<MaskCoordinate>
                {
                    new MaskCoordinate { X = 100, Y = 200 },
                    new MaskCoordinate { X = 300, Y = 400 }
                }
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<UpsertCameraMaskCommand>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.UpsertImageMask(cameraMask, cancellationToken);

            // Assert
            result.Should().BeTrue();
            await Mediator.Received(1).Send(
                Arg.Is<UpsertCameraMaskCommand>(cmd => 
                    cmd.CameraMask.CameraId == cameraMask.CameraId), 
                cancellationToken);
        }

        [Test]
        public async Task UpsertImageMask_ReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var cameraMask = new CameraMask
            {
                CameraId = Guid.NewGuid(),
                Coordinates = new List<MaskCoordinate>()
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<UpsertCameraMaskCommand>(), cancellationToken)
                .Returns(false);

            // Act
            var result = await _controller.UpsertImageMask(cameraMask, cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task UpsertImageMask_NullMask_CallsCommandWithNull()
        {
            // Arrange
            CameraMask nullMask = null;
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<UpsertCameraMaskCommand>(), cancellationToken)
                .Returns(false);

            // Act
            var result = await _controller.UpsertImageMask(nullMask, cancellationToken);

            // Assert
            result.Should().BeFalse();
            await Mediator.Received(1).Send(
                Arg.Is<UpsertCameraMaskCommand>(cmd => cmd.CameraMask == null), 
                cancellationToken);
        }

        [Test]
        public async Task GetImageMaskCoordinates_ValidId_ReturnsCorrectResult()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var expectedCoordinates = new List<MaskCoordinate>
            {
                new MaskCoordinate { X = 100, Y = 200 },
                new MaskCoordinate { X = 300, Y = 400 }
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCameraMaskQuery>(), cancellationToken)
                .Returns(expectedCoordinates);

            // Act
            var result = await _controller.GetImageMaskCoordinates(cameraId, cancellationToken);

            // Assert
            result.Should().HaveCount(2);
            result[0].X.Should().Be(100);
            result[0].Y.Should().Be(200);
            result[1].X.Should().Be(300);
            result[1].Y.Should().Be(400);
            
            await Mediator.Received(1).Send(
                Arg.Is<GetCameraMaskQuery>(q => q.CameraId == cameraId), 
                cancellationToken);
        }

        [Test]
        public async Task GetImageMaskCoordinates_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCameraMaskQuery>(), cancellationToken)
                .Returns(new List<MaskCoordinate>());

            // Act
            var result = await _controller.GetImageMaskCoordinates(cameraId, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetPlateCaptures_ValidId_ReturnsCorrectResult()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var expectedCaptures = new List<string> { "capture1.jpg", "capture2.jpg", "capture3.jpg" };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetPlateCapturesQuery>(), cancellationToken)
                .Returns(expectedCaptures);

            // Act
            var result = await _controller.GetPlateCaptures(cameraId, cancellationToken);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain("capture1.jpg");
            result.Should().Contain("capture2.jpg");
            result.Should().Contain("capture3.jpg");
            
            await Mediator.Received(1).Send(
                Arg.Is<GetPlateCapturesQuery>(q => q.CameraId == cameraId), 
                cancellationToken);
        }

        [Test]
        public async Task GetPlateCaptures_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetPlateCapturesQuery>(), cancellationToken)
                .Returns(new List<string>());

            // Act
            var result = await _controller.GetPlateCaptures(cameraId, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetPlateCaptures_NullResult_ReturnsNull()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetPlateCapturesQuery>(), cancellationToken)
                .Returns((List<string>)null);

            // Act
            var result = await _controller.GetPlateCaptures(cameraId, cancellationToken);

            // Assert
            result.Should().BeNull();
        }
    }
} 