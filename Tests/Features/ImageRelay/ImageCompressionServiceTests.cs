using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using System.Net;
using Tests.TestHelpers;
namespace Tests.Features.ImageRelay
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ImageCompressionServiceTests : TestBase
    {
        private ImageCompressionService _service;
        private IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        private TestHttpMessageHandler _httpMessageHandler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _httpMessageHandler = new TestHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler);
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _httpClientFactory.CreateClient().Returns(_httpClient);
            _service = new ImageCompressionService(_httpClientFactory);
        }

        [TearDown]
        public override void TearDown()
        {
            _httpClient?.Dispose();
            _httpMessageHandler?.Dispose();
            base.TearDown();
        }

        #region GetImageFromAgentAsync Tests

        [Test]
        public async Task GetImageFromAgentAsync_ValidAgentAndImageId_ReturnsImageBytes()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);
            var imageId = "test-image-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedImageBytes);

            // Act
            var result = await _service.GetImageFromAgentAsync(agent, imageId, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(expectedImageBytes);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/img/{imageId}");
        }

        [Test]
        public async Task GetImageFromAgentAsync_CompressionEnabled_ReturnsCompressedImage()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", true);
            var imageId = "test-image-123";
            var originalImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, originalImageBytes);

            // Act
            var result = await _service.GetImageFromAgentAsync(agent, imageId, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/img/{imageId}");
        }

        [Test]
        public async Task GetImageFromAgentAsync_CompressionDisabled_ReturnsOriginalImage()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);
            var imageId = "test-image-123";
            var originalImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, originalImageBytes);

            // Act
            var result = await _service.GetImageFromAgentAsync(agent, imageId, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(originalImageBytes);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/img/{imageId}");
        }

        [Test]
        public async Task GetImageFromAgentAsync_NullAgent_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "test-image-123";
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _service.GetImageFromAgentAsync(null, imageId, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Agent not configured");
        }

        [Test]
        public async Task GetImageFromAgentAsync_AgentWithNullEndpointUrl_ThrowsArgumentException()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression(null);
            var imageId = "test-image-123";
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _service.GetImageFromAgentAsync(agent, imageId, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Agent not configured");
        }

        [Test]
        public async Task GetImageFromAgentAsync_AgentWithEmptyEndpointUrl_ThrowsArgumentException()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("");
            var imageId = "test-image-123";
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _service.GetImageFromAgentAsync(agent, imageId, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Agent not configured");
        }

        [Test]
        public async Task GetImageFromAgentAsync_HttpNotFound_ThrowsArgumentException()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local");
            var imageId = "non-existent-image";
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

            // Act & Assert
            await FluentActions.Invoking(() => _service.GetImageFromAgentAsync(agent, imageId, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Image not found for that id.");
        }

        [Test]
        public async Task GetImageFromAgentAsync_HttpInternalServerError_ThrowsArgumentException()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local");
            var imageId = "test-image-123";
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError);

            // Act & Assert
            await FluentActions.Invoking(() => _service.GetImageFromAgentAsync(agent, imageId, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Image not found for that id.");
        }

        #endregion

        #region GetCropImageFromAgentAsync Tests

        [Test]
        public async Task GetCropImageFromAgentAsync_ValidAgentAndImageId_ReturnsCropImageBytes()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);
            var imageId = "test-image-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedImageBytes);

            // Act
            var result = await _service.GetCropImageFromAgentAsync(agent, imageId, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(expectedImageBytes);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/crop/{imageId}");
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_WithPlateCoordinates_ReturnsImageWithCoordinates()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);
            var imageId = "test-image-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedImageBytes);

            // Act
            var result = await _service.GetCropImageFromAgentAsync(agent, imageId, plateCoordinates, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(expectedImageBytes);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/crop/{imageId}?{plateCoordinates}");
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_WithNullPlateCoordinates_ReturnsImageWithoutCoordinates()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);
            var imageId = "test-image-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedImageBytes);

            // Act
            var result = await _service.GetCropImageFromAgentAsync(agent, imageId, null, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(expectedImageBytes);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/crop/{imageId}");
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_WithEmptyPlateCoordinates_ReturnsImageWithoutCoordinates()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);
            var imageId = "test-image-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedImageBytes);

            // Act
            var result = await _service.GetCropImageFromAgentAsync(agent, imageId, "", cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(expectedImageBytes);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/crop/{imageId}");
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_CompressionEnabled_ReturnsCompressedImage()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", true);
            var imageId = "test-image-123";
            var originalImageBytes = TestDataFactory.CreateTestJpegBytes();
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, originalImageBytes);

            // Act
            var result = await _service.GetCropImageFromAgentAsync(agent, imageId, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            _httpMessageHandler.LastRequestUri.Should().Be($"{agent.EndpointUrl}/crop/{imageId}");
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_NullAgent_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "test-image-123";
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _service.GetCropImageFromAgentAsync(null, imageId, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Agent not configured");
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_HttpNotFound_ThrowsArgumentException()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local");
            var imageId = "non-existent-image";
            var cancellationToken = GetCancellationToken();

            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

            // Act & Assert
            await FluentActions.Invoking(() => _service.GetCropImageFromAgentAsync(agent, imageId, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Image not found for that id.");
        }

        #endregion

        #region CompressImage Tests

        [Test]
        public void CompressImage_ValidJpegBytes_ReturnsCompressedBytes()
        {
            // Arrange
            var originalImageBytes = TestDataFactory.CreateTestJpegBytes();

            // Act
            var result = ImageCompressionService.CompressImage(originalImageBytes);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public void CompressImage_InvalidImageBytes_ReturnsOriginalBytes()
        {
            // Arrange
            var invalidImageBytes = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = ImageCompressionService.CompressImage(invalidImageBytes);

            // Assert
            result.Should().BeEquivalentTo(invalidImageBytes);
        }

        [Test]
        public void CompressImage_NullImageBytes_ReturnsNull()
        {
            // Arrange
            byte[] nullImageBytes = null;

            // Act
            var result = ImageCompressionService.CompressImage(nullImageBytes);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void CompressImage_EmptyImageBytes_ReturnsEmptyArray()
        {
            // Arrange
            var emptyImageBytes = new byte[0];

            // Act
            var result = ImageCompressionService.CompressImage(emptyImageBytes);

            // Assert
            result.Should().BeEquivalentTo(emptyImageBytes);
        }

        #endregion

        #region Test Helper Class

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private HttpStatusCode _statusCode = HttpStatusCode.OK;
            private byte[] _responseContent = Array.Empty<byte>();
            public string LastRequestUri { get; private set; }

            public void SetupResponse(HttpStatusCode statusCode, byte[] content = null)
            {
                _statusCode = statusCode;
                _responseContent = content ?? Array.Empty<byte>();
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
            {
                LastRequestUri = request.RequestUri.ToString();
                
                var response = new HttpResponseMessage(_statusCode);
                if (_responseContent != null)
                {
                    response.Content = new ByteArrayContent(_responseContent);
                }
                
                return Task.FromResult(response);
            }
        }

        #endregion
    }
} 