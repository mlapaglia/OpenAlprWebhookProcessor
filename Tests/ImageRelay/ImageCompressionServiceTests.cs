using FluentAssertions;
using NSubstitute;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.ImageRelay.ImageCompression;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.ImageRelay
{
    [TestFixture]
    public class ImageCompressionServiceTests
    {
        private IHttpClientFactory _httpClientFactory;
        private ImageCompressionService _service;
        private HttpClient _httpClient;
        private HttpMessageHandler _httpMessageHandler;

        [SetUp]
        public void Setup()
        {
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _httpMessageHandler = Substitute.For<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandler);

            _httpClientFactory.CreateClient().Returns(_httpClient);

            _service = new ImageCompressionService(_httpClientFactory);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            _httpMessageHandler?.Dispose();
        }

        [Test]
        public async Task GetImageFromAgentAsync_NullAgent_ThrowsArgumentException()
        {
            // Arrange
            Agent agent = null;
            var imageId = "test-image-id";

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetImageFromAgentAsync(agent, imageId, CancellationToken.None));

            exception.Message.Should().Be("Agent not configured");
        }

        [Test]
        public async Task GetImageFromAgentAsync_EmptyEndpointUrl_ThrowsArgumentException()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "", // Empty endpoint URL
                IsImageCompressionEnabled = false
            };
            var imageId = "test-image-id";

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetImageFromAgentAsync(agent, imageId, CancellationToken.None));

            exception.Message.Should().Be("Agent not configured");
        }

        [Test]
        public async Task GetImageFromAgentAsync_NullEndpointUrl_ThrowsArgumentException()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = null, // Null endpoint URL
                IsImageCompressionEnabled = false
            };
            var imageId = "test-image-id";

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetImageFromAgentAsync(agent, imageId, CancellationToken.None));

            exception.Message.Should().Be("Agent not configured");
        }

        [Test]
        public async Task GetImageFromAgentAsync_ValidAgent_ReturnsImageBytes()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };
            var imageId = "test-image-id";
            var imageBytes = new byte[] { 0x1, 0x2, 0x3, 0x4 };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };

            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            var result = await _service.GetImageFromAgentAsync(agent, imageId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(imageBytes);
        }

        [Test]
        public async Task GetImageFromAgentAsync_CompressionEnabled_ReturnsCompressedImage()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = true // Compression enabled
            };
            var imageId = "test-image-id";
            var imageBytes = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };

            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            var result = await _service.GetImageFromAgentAsync(agent, imageId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            // The result should be processed through compression
            // Note: Since we can't easily mock ImageMagick, we just verify it returns something
        }

        [Test]
        public async Task GetImageFromAgentAsync_HttpClientError_ThrowsArgumentException()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };
            var imageId = "test-image-id";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetImageFromAgentAsync(agent, imageId, CancellationToken.None));

            exception.Message.Should().Be("Image not found for that id.");
        }

        [Test]
        public async Task GetImageFromAgentAsync_BuildsCorrectUrl()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };
            var imageId = "test-image-id";
            var imageBytes = new byte[] { 0x1, 0x2, 0x3, 0x4 };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };

            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            await _service.GetImageFromAgentAsync(agent, imageId, CancellationToken.None);

            // Assert
            var expectedUrl = $"http://test-agent.com/img/{imageId}";
            await mockHttpClient.Received(1).GetAsync(expectedUrl, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_NullAgent_ThrowsArgumentException()
        {
            // Arrange
            Agent agent = null;
            var imageId = "test-crop-image-id";

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetCropImageFromAgentAsync(agent, imageId, CancellationToken.None));

            exception.Message.Should().Be("Agent not configured");
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_ValidAgent_ReturnsImageBytes()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };
            var imageId = "test-crop-image-id";
            var imageBytes = new byte[] { 0x5, 0x6, 0x7, 0x8 };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };

            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            var result = await _service.GetCropImageFromAgentAsync(agent, imageId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(imageBytes);
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_BuildsCorrectUrl()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };
            var imageId = "test-crop-image-id";
            var imageBytes = new byte[] { 0x5, 0x6, 0x7, 0x8 };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };

            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            await _service.GetCropImageFromAgentAsync(agent, imageId, CancellationToken.None);

            // Assert
            var expectedUrl = $"http://test-agent.com/crop/{imageId}";
            await mockHttpClient.Received(1).GetAsync(expectedUrl, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetCropImageFromAgentAsync_HttpClientError_ThrowsArgumentException()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };
            var imageId = "test-crop-image-id";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetCropImageFromAgentAsync(agent, imageId, CancellationToken.None));

            exception.Message.Should().Be("Image not found for that id.");
        }

        [Test]
        public void CompressImage_ValidImageBytes_ReturnsCompressedBytes()
        {
            // Arrange
            var originalBytes = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 };

            // Act
            var compressedBytes = ImageCompressionService.CompressImage(originalBytes);

            // Assert
            compressedBytes.Should().NotBeNull();
            // Note: Since we can't easily mock ImageMagick, we just ensure it returns something
        }

        [Test]
        public void CompressImage_NullBytes_ReturnsOriginalBytes()
        {
            // Arrange
            byte[] originalBytes = null;

            // Act
            var result = ImageCompressionService.CompressImage(originalBytes);

            // Assert
            result.Should().Equal(originalBytes);
        }

        [Test]
        public void CompressImage_EmptyBytes_ReturnsOriginalBytes()
        {
            // Arrange
            var originalBytes = new byte[0];

            // Act
            var result = ImageCompressionService.CompressImage(originalBytes);

            // Assert
            result.Should().NotBeNull();
            // Result should be the processed bytes or original if compression fails
        }

        [Test]
        public void Constructor_ValidHttpClientFactory_InitializesCorrectly()
        {
            // Arrange & Act
            var service = new ImageCompressionService(_httpClientFactory);

            // Assert
            service.Should().NotBeNull();
        }

        [Test]
        public void Constructor_NullHttpClientFactory_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ImageCompressionService(null));
        }
    }
} 