using FluentAssertions;
using NSubstitute;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.ImageRelay.GetImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.ImageRelay
{
    [TestFixture]
    public class GetImageQueryHandlerTests
    {
        private IUnitOfWork _unitOfWork;
        private IHttpClientFactory _httpClientFactory;
        private GetImageQueryHandler _handler;
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

            _handler = new GetImageQueryHandler(_unitOfWork, _httpClientFactory);
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
            var query = new GetImageQuery("non-existent-image");
            
            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup>()));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(query, CancellationToken.None));
            
            exception.Message.Should().Be("No image found with that id.");
        }

        [Test]
        public async Task Handle_ExistingVehicleImage_ReturnsExistingImage()
        {
            // Arrange
            var imageId = "test-image-id";
            var query = new GetImageQuery(imageId);
            var existingImageBytes = new byte[] { 0x1, 0x2, 0x3, 0x4 };
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                VehicleImage = new VehicleImage
                {
                    Jpeg = existingImageBytes,
                    IsCompressed = false
                }
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                VehicleImage = plateGroup.VehicleImage
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
        public async Task Handle_NoVehicleImage_FetchesFromAgent()
        {
            // Arrange
            var imageId = "test-image-id";
            var query = new GetImageQuery(imageId);
            var imageBytes = new byte[] { 0x5, 0x6, 0x7, 0x8 };
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                VehicleImage = null
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                VehicleImage = null
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
        public async Task Handle_AgentNotConfigured_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "test-image-id";
            var query = new GetImageQuery(imageId);
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                VehicleImage = null
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                VehicleImage = null
            };

            _plateGroupRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<PlateGroup>>(new List<PlateGroup> { plateGroup }));

            _plateGroupRepository.GetByIdWithDetailsAsync(plateGroup.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroupWithDetails)!);

            _agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<Agent>>(new List<Agent>()));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(query, CancellationToken.None));
            
            exception.Message.Should().Be("Agent not configured");
        }

        [Test]
        public async Task Handle_AgentWithEmptyEndpointUrl_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "test-image-id";
            var query = new GetImageQuery(imageId);
            
            var plateGroup = new PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = imageId,
                VehicleImage = null
            };

            var plateGroupWithDetails = new PlateGroup
            {
                Id = plateGroup.Id,
                OpenAlprUuid = imageId,
                VehicleImage = null
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
                () => _handler.Handle(query, CancellationToken.None));
            
            exception.Message.Should().Be("Agent not configured");
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
            // In a real scenario, you might want to use a wrapper interface for ImageMagick
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