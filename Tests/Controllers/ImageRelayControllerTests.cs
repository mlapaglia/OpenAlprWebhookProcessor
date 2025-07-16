using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.ImageRelay;
using OpenAlprWebhookProcessor.Features.ImageRelay.GetImage;
using OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotRelay;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class ImageRelayControllerTests : TestBase
    {
        private ImageRelayController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new ImageRelayController(Mediator);
        }

        [Test]
        public async Task GetImage_ValidImageId_ReturnsFileResult()
        {
            // Arrange
            var imageId = "test-image-123";
            var imageBytes = new byte[] { 1, 2, 3, 4, 5 };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(imageBytes));

            // Act
            var result = await _controller.GetImage(imageId, cancellationToken);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult.ContentType.Should().Be("image/jpeg");
        }

        [Test]
        public async Task GetImage_InvalidImageId_ReturnsNotFound()
        {
            // Arrange
            var imageId = "invalid-image-id";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetImageQuery>(), cancellationToken)
                .Throws(new Exception("Image not found"));

            // Act
            var result = await _controller.GetImage(imageId, cancellationToken);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetImage_CallsCorrectQuery()
        {
            // Arrange
            var imageId = "test-image-123";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetImage(imageId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetImageQuery>(q => q.ImageId == imageId), 
                cancellationToken);
        }

        [Test]
        public async Task GetCropImage_ValidImageId_ReturnsFileResult()
        {
            // Arrange
            var imageId = "test-crop-image-123";
            var cropImageBytes = new byte[] { 5, 4, 3, 2, 1 };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCropImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(cropImageBytes));

            // Act
            var result = await _controller.GetCropImage(imageId, cancellationToken);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult.ContentType.Should().Be("image/jpeg");
        }

        [Test]
        public async Task GetCropImage_InvalidImageId_ReturnsNotFound()
        {
            // Arrange
            var imageId = "invalid-crop-image-id";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCropImageQuery>(), cancellationToken)
                .Throws(new Exception("Crop image not found"));

            // Act
            var result = await _controller.GetCropImage(imageId, cancellationToken);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetCropImage_CallsCorrectQuery()
        {
            // Arrange
            var imageId = "test-crop-image-123";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCropImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetCropImage(imageId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetCropImageQuery>(q => q.ImageId == imageId), 
                cancellationToken);
        }

        [Test]
        public async Task GetSnapshot_ValidCameraId_ReturnsFileResult()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var snapshotBytes = new byte[] { 10, 20, 30, 40, 50 };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetSnapshotQuery>(), cancellationToken)
                .Returns(new MemoryStream(snapshotBytes));

            // Act
            var result = await _controller.GetSnapshot(cameraId, cancellationToken);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult.ContentType.Should().Be("image/jpeg");
        }

        [Test]
        public async Task GetSnapshot_InvalidCameraId_ReturnsNotFound()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetSnapshotQuery>(), cancellationToken)
                .Throws(new Exception("Camera not found"));

            // Act
            var result = await _controller.GetSnapshot(cameraId, cancellationToken);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetSnapshot_CallsCorrectQuery()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetSnapshotQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetSnapshot(cameraId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetSnapshotQuery>(q => q.CameraId == cameraId), 
                cancellationToken);
        }

        [Test]
        public async Task GetImage_NullImageId_CallsQueryWithNullId()
        {
            // Arrange
            string imageId = null;
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetImage(imageId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetImageQuery>(q => q.ImageId == null), 
                cancellationToken);
        }

        [Test]
        public async Task GetImage_EmptyImageId_CallsQueryWithEmptyId()
        {
            // Arrange
            var imageId = "";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetImage(imageId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetImageQuery>(q => q.ImageId == ""), 
                cancellationToken);
        }

        [Test]
        public async Task GetCropImage_NullImageId_CallsQueryWithNullId()
        {
            // Arrange
            string imageId = null;
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCropImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetCropImage(imageId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetCropImageQuery>(q => q.ImageId == null), 
                cancellationToken);
        }

        [Test]
        public async Task GetCropImage_EmptyImageId_CallsQueryWithEmptyId()
        {
            // Arrange
            var imageId = "";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCropImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetCropImage(imageId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetCropImageQuery>(q => q.ImageId == ""), 
                cancellationToken);
        }

        [Test]
        public async Task GetSnapshot_EmptyGuidCameraId_CallsQueryWithEmptyGuid()
        {
            // Arrange
            var cameraId = Guid.Empty;
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetSnapshotQuery>(), cancellationToken)
                .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            // Act
            await _controller.GetSnapshot(cameraId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetSnapshotQuery>(q => q.CameraId == Guid.Empty), 
                cancellationToken);
        }

        [Test]
        public async Task GetImage_ReturnsCorrectContentType()
        {
            // Arrange
            var imageId = "test-image-123";
            var imageBytes = new byte[] { 1, 2, 3, 4, 5 };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(imageBytes));

            // Act
            var result = await _controller.GetImage(imageId, cancellationToken);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult.ContentType.Should().Be("image/jpeg");
        }

        [Test]
        public async Task GetCropImage_ReturnsCorrectContentType()
        {
            // Arrange
            var imageId = "test-crop-image-123";
            var cropImageBytes = new byte[] { 5, 4, 3, 2, 1 };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetCropImageQuery>(), cancellationToken)
                .Returns(new MemoryStream(cropImageBytes));

            // Act
            var result = await _controller.GetCropImage(imageId, cancellationToken);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult.ContentType.Should().Be("image/jpeg");
        }

        [Test]
        public async Task GetSnapshot_ReturnsCorrectContentType()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var snapshotBytes = new byte[] { 10, 20, 30, 40, 50 };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetSnapshotQuery>(), cancellationToken)
                .Returns(new MemoryStream(snapshotBytes));

            // Act
            var result = await _controller.GetSnapshot(cameraId, cancellationToken);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult.ContentType.Should().Be("image/jpeg");
        }
    }
} 