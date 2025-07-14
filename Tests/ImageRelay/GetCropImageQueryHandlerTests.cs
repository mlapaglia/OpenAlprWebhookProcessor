using FluentAssertions;
using NSubstitute;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.ImageRelay.GetImage;
using System.Net;

namespace Tests.ImageRelay
{
    [TestFixture]
    public class GetCropImageQueryHandlerTests
    {
        private IUnitOfWork _unitOfWork;
        private IHttpClientFactory _httpClientFactory;
        private GetCropImageQueryHandler _handler;
        private IPlateGroupRepository _plateGroupRepository;
        private IRepository<Agent> _agentRepository;
        private HttpClient _httpClient;
        private HttpMessageHandler _httpMessageHandler;

        [SetUp]
        public void Setup()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            _agentRepository = Substitute.For<IRepository<Agent>>();
            _httpMessageHandler = Substitute.For<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandler);

            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _unitOfWork.Agents.Returns(_agentRepository);
            _httpClientFactory.CreateClient().Returns(_httpClient);

            _handler = new GetCropImageQueryHandler(_unitOfWork, _httpClientFactory);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            _httpMessageHandler?.Dispose();
            _unitOfWork?.Dispose();
        }

        [Test]
        public async Task Handle_PlateGroupNotFound_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetCropImageQuery("non-existent-image");
            
            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup>()));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(query, CancellationToken.None));
            
            exception.Message.Should().Be("No image found with that id.");
        }

        [Test]
        public async Task Handle_ExistingPlateImage_ReturnsExistingImage()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var query = new GetCropImageQuery(imageId);
            var existingImageBytes = new byte[] { 0x1, 0x2, 0x3, 0x4 };
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                PlateImage = new PlateImage
                {
                    Jpeg = existingImageBytes,
                    IsCompressed = false
                }
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                PlateImage = plateGroup.PlateImage
            };

            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup> { plateGroup }));

            _plateGroupRepository.GetByIdWithDetailsAsync(plateGroup.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroupWithDetails)!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            var memoryStream = result as MemoryStream;
            memoryStream!.ToArray().Should().BeEquivalentTo(existingImageBytes);
        }

        [Test]
        public async Task Handle_NoPlateImage_FetchesFromAgent()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var query = new GetCropImageQuery(imageId);
            var imageBytes = new byte[] { 0x5, 0x6, 0x7, 0x8 };
            var plateCoordinates = "123,456,789,101";
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                PlateImage = null,
                PlateCoordinates = plateCoordinates
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                PlateImage = null,
                PlateCoordinates = plateCoordinates
            };

            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };

            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup> { plateGroup }));

            _plateGroupRepository.GetByIdWithDetailsAsync(plateGroup.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroupWithDetails)!);

            _agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<Agent>>(new List<Agent> { agent }));

            // Mock HttpClient response
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };
            
            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            // Verify that SaveChanges was called to save the new image
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_NoPlateImage_BuildsCorrectCropUrl()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var query = new GetCropImageQuery(imageId);
            var imageBytes = new byte[] { 0x5, 0x6, 0x7, 0x8 };
            var plateCoordinates = "100,200,300,400";
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                PlateImage = null,
                PlateCoordinates = plateCoordinates
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                PlateImage = null,
                PlateCoordinates = plateCoordinates
            };

            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = false
            };

            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup> { plateGroup }));

            _plateGroupRepository.GetByIdWithDetailsAsync(plateGroup.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroupWithDetails)!);

            _agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<Agent>>(new List<Agent> { agent }));

            // Mock HttpClient response
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };
            
            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            var expectedUrl = $"http://test-agent.com/crop/{imageId}?{plateCoordinates}";
            await mockHttpClient.Received(1).GetAsync(expectedUrl, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_AgentNotConfigured_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var query = new GetCropImageQuery(imageId);
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                PlateImage = null
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                PlateImage = null
            };

            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup> { plateGroup }));

            _plateGroupRepository.GetByIdWithDetailsAsync(plateGroup.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroupWithDetails)!);

            _agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<Agent>>(new List<Agent>()));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _handler.Handle(query, CancellationToken.None));
            
            exception.Message.Should().Be("Agent not configured");
        }

        [Test]
        public async Task Handle_AgentWithEmptyEndpointUrl_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var query = new GetCropImageQuery(imageId);
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                PlateImage = null
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                PlateImage = null
            };

            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "", // Empty endpoint URL
                IsImageCompressionEnabled = false
            };

            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup> { plateGroup }));

            _plateGroupRepository.GetByIdWithDetailsAsync(plateGroup.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroupWithDetails)!);

            _agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<Agent>>(new List<Agent> { agent }));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _handler.Handle(query, CancellationToken.None));
            
            exception.Message.Should().Be("Agent not configured");
        }

        [Test]
        public async Task Handle_CompressionEnabled_ReturnsCompressedImage()
        {
            // Arrange
            var imageId = "test-crop-image-id";
            var query = new GetCropImageQuery(imageId);
            var imageBytes = new byte[] { 0x5, 0x6, 0x7, 0x8 };
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                PlateImage = null,
                PlateCoordinates = "123,456,789,101"
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                PlateImage = null,
                PlateCoordinates = plateGroup.PlateCoordinates
            };

            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.com",
                IsImageCompressionEnabled = true // Compression enabled
            };

            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup> { plateGroup }));

            _plateGroupRepository.GetByIdWithDetailsAsync(plateGroup.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroupWithDetails)!);

            _agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<Agent>>(new List<Agent> { agent }));

            // Mock HttpClient response
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imageBytes)
            };
            
            var mockHttpClient = Substitute.For<HttpClient>();
            _httpClientFactory.CreateClient().Returns(mockHttpClient);
            mockHttpClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(mockResponse));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            // Verify that SaveChanges was called
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void CompressImage_ValidImageBytes_ReturnsCompressedBytes()
        {
            // Arrange
            var originalBytes = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 };

            // Act
            var compressedBytes = OpenAlprWebhookProcessor.ImageRelay.ImageCompression.ImageCompressionService.CompressImage(originalBytes);

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
            var result = OpenAlprWebhookProcessor.ImageRelay.ImageCompression.ImageCompressionService.CompressImage(originalBytes);

            // Assert
            result.Should().Equal(originalBytes);
        }
    }
} 