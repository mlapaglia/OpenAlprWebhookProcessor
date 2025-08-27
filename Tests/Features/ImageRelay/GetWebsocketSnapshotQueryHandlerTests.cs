using AwesomeAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.ImageRelay.WebsocketSnapshotRelay;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.ImageRelay
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetWebsocketSnapshotQueryHandlerTests : TestBase
    {
        private GetWebsocketSnapshotQueryHandler _handler;
        private IWebsocketClientOrganizer _mockWebsocketClientOrganizer;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockWebsocketClientOrganizer = Substitute.For<IWebsocketClientOrganizer>();
            _handler = new GetWebsocketSnapshotQueryHandler(_mockWebsocketClientOrganizer);
        }

        [Test]
        public async Task Handle_ValidParameters_ReturnsStreamFromBase64()
        {
            // Arrange
            var agentId = "test-agent-123";
            var cameraId = 123;
            var testImageBytes = TestDataFactory.CreateTestJpegBytes();
            var base64Image = Convert.ToBase64String(testImageBytes);
            
            var response = new ImageDownloadResponse
            {
                Image = base64Image,
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns(response);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(testImageBytes);
        }

        [Test]
        public async Task Handle_NullResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            var agentId = "test-agent-123";
            var cameraId = 123;

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns((ImageDownloadResponse)null);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Failed to get snapshot from agent");
        }

        [Test]
        public async Task Handle_EmptyImageData_ThrowsInvalidOperationException()
        {
            // Arrange
            var agentId = "test-agent-123";
            var cameraId = 123;
            
            var response = new ImageDownloadResponse
            {
                Image = "",
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns(response);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Agent returned empty image data");
        }

        [Test]
        public async Task Handle_NullImageData_ThrowsInvalidOperationException()
        {
            // Arrange
            var agentId = "test-agent-123";
            var cameraId = 123;
            
            var response = new ImageDownloadResponse
            {
                Image = null,
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns(response);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Agent returned empty image data");
        }

        [Test]
        public async Task Handle_InvalidBase64_ThrowsInvalidOperationException()
        {
            // Arrange
            var agentId = "test-agent-123";
            var cameraId = 123;
            
            var response = new ImageDownloadResponse
            {
                Image = "invalid-base64-data",
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns(response);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Invalid image data received from agent");
        }

        [Test]
        public async Task Handle_NullAgentId_CallsWebsocketOrganizerWithNullId()
        {
            // Arrange
            var agentId = (string)null;
            var cameraId = 123;
            var testImageBytes = TestDataFactory.CreateTestJpegBytes();
            var base64Image = Convert.ToBase64String(testImageBytes);
            
            var response = new ImageDownloadResponse
            {
                Image = base64Image,
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns(response);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _mockWebsocketClientOrganizer.Received(1)
                .GetCameraSnapshotAsync(agentId, cameraId, cancellationToken);
        }

        [Test]
        public async Task Handle_EmptyAgentId_CallsWebsocketOrganizerWithEmptyId()
        {
            // Arrange
            var agentId = "";
            var cameraId = 123;
            var testImageBytes = TestDataFactory.CreateTestJpegBytes();
            var base64Image = Convert.ToBase64String(testImageBytes);
            
            var response = new ImageDownloadResponse
            {
                Image = base64Image,
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns(response);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _mockWebsocketClientOrganizer.Received(1)
                .GetCameraSnapshotAsync(agentId, cameraId, cancellationToken);
        }

        [Test]
        public void Constructor_NullWebsocketClientOrganizer_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new GetWebsocketSnapshotQueryHandler(null))
                .Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'websocketClientOrganizer')");
        }

        [Test]
        public async Task Handle_WebsocketOrganizerThrowsException_PropagatesException()
        {
            // Arrange
            var agentId = "test-agent-123";
            var cameraId = 123;

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Agent connection failed"));

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<Exception>()
                .WithMessage("Agent connection failed");
        }

        [Test]
        public async Task Handle_ValidQuery_CallsWebsocketOrganizerWithCorrectParameters()
        {
            // Arrange
            var agentId = "test-agent-123";
            var cameraId = 123;
            var testImageBytes = TestDataFactory.CreateTestJpegBytes();
            var base64Image = Convert.ToBase64String(testImageBytes);
            
            var response = new ImageDownloadResponse
            {
                Image = base64Image,
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            _mockWebsocketClientOrganizer.GetCameraSnapshotAsync(agentId, cameraId, Arg.Any<CancellationToken>())
                .Returns(response);

            var query = new GetWebsocketSnapshotQuery(agentId, cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            await _mockWebsocketClientOrganizer.Received(1)
                .GetCameraSnapshotAsync(agentId, cameraId, cancellationToken);
        }
    }
}
