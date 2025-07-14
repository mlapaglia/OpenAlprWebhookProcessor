using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OpenAlprWebhookProcessor.ImageRelay;
using OpenAlprWebhookProcessor.ImageRelay.GetImage;
using OpenAlprWebhookProcessor.ImageRelay.SnapshotRelay;

namespace Tests.ImageRelay
{
    [TestFixture]
    public class ImageRelayControllerTests
    {
        private IMediator _mediator;
        private ImageRelayController _controller;

        [SetUp]
        public void Setup()
        {
            _mediator = Substitute.For<IMediator>();
            _controller = new ImageRelayController(_mediator);
        }

        [Test]
        public async Task GetImage_ValidImageId_ReturnsFileResult()
        {
            // Arrange
            var imageId = "test-image-id";
            var mockStream = new MemoryStream(new byte[] { 0x1, 0x2, 0x3 });
            var query = new GetImageQuery(imageId);
            
            _mediator.Send(Arg.Is<GetImageQuery>(q => q.ImageId == imageId), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(mockStream));

            // Act
            var result = await _controller.GetImage(imageId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult!.ContentType.Should().Be("image/jpeg");
            fileResult.FileStream.Should().BeSameAs(mockStream);
        }

        [Test]
        public async Task GetImage_HandlerThrowsException_ReturnsNotFound()
        {
            // Arrange
            var imageId = "non-existent-image";
            
            _mediator.Send(Arg.Any<GetImageQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Stream>(new ArgumentException("No image found")));

            // Act
            var result = await _controller.GetImage(imageId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetCropImage_ValidImageId_ReturnsFileResult()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var mockStream = new MemoryStream(new byte[] { 0x4, 0x5, 0x6 });
            
            _mediator.Send(Arg.Is<GetCropImageQuery>(q => q.ImageId == imageId), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(mockStream));

            // Act
            var result = await _controller.GetCropImage(imageId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult!.ContentType.Should().Be("image/jpeg");
            fileResult.FileStream.Should().BeSameAs(mockStream);
        }

        [Test]
        public async Task GetCropImage_HandlerThrowsException_ReturnsNotFound()
        {
            // Arrange
            var imageId = "non-existent-crop-image";
            
            _mediator.Send(Arg.Any<GetCropImageQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Stream>(new ArgumentException("No image found")));

            // Act
            var result = await _controller.GetCropImage(imageId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetSnapshot_ValidCameraId_ReturnsFileResult()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var mockStream = new MemoryStream(new byte[] { 0x7, 0x8, 0x9 });
            
            _mediator.Send(Arg.Is<GetSnapshotQuery>(q => q.CameraId == cameraId), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(mockStream));

            // Act
            var result = await _controller.GetSnapshot(cameraId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult!.ContentType.Should().Be("image/jpeg");
            fileResult.FileStream.Should().BeSameAs(mockStream);
        }

        [Test]
        public async Task GetSnapshot_HandlerThrowsException_ReturnsNotFound()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            
            _mediator.Send(Arg.Any<GetSnapshotQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Stream>(new TimeoutException("Unable to get image from camera")));

            // Act
            var result = await _controller.GetSnapshot(cameraId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetImage_CallsMediator_WithCorrectQuery()
        {
            // Arrange
            var imageId = "test-image-id";
            var mockStream = new MemoryStream();
            
            _mediator.Send(Arg.Any<GetImageQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(mockStream));

            // Act
            await _controller.GetImage(imageId, CancellationToken.None);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Is<GetImageQuery>(q => q.ImageId == imageId),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetCropImage_CallsMediator_WithCorrectQuery()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var mockStream = new MemoryStream();
            
            _mediator.Send(Arg.Any<GetCropImageQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(mockStream));

            // Act
            await _controller.GetCropImage(imageId, CancellationToken.None);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Is<GetCropImageQuery>(q => q.ImageId == imageId),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetSnapshot_CallsMediator_WithCorrectQuery()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var mockStream = new MemoryStream();
            
            _mediator.Send(Arg.Any<GetSnapshotQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(mockStream));

            // Act
            await _controller.GetSnapshot(cameraId, CancellationToken.None);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Is<GetSnapshotQuery>(q => q.CameraId == cameraId),
                Arg.Any<CancellationToken>());
        }
    }
} 