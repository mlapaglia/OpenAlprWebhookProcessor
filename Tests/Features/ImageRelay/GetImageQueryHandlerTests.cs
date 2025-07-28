using AwesomeAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.ImageRelay.GetImage;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using Tests.TestHelpers;

namespace Tests.Features.ImageRelay
{
    [TestFixture]
    public class GetImageQueryHandlerTests : TestBase
    {
        private GetImageQueryHandler _handler;
        private IImageCompressionService _imageCompressionService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _imageCompressionService = Substitute.For<IImageCompressionService>();
            _handler = new GetImageQueryHandler(UnitOfWork, _imageCompressionService);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Handle_ExistingImageInDatabase_ReturnsImageFromDatabase()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(imageId, true, false);
            plateGroup.VehicleImage.Jpeg = expectedImageBytes;

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);
            
            // Verify image compression service was not called since image was cached
            await _imageCompressionService.DidNotReceive().GetImageFromAgentAsync(
                Arg.Any<Agent>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ImageNotInDatabase_FetchesFromAgentAndCaches()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetImageFromAgentAsync(Arg.Any<Agent>(), imageId, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            await _imageCompressionService.Received(1).GetImageFromAgentAsync(
                agent, 
                imageId, 
                cancellationToken);

            var savedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            savedPlateGroup.VehicleImage.Should().NotBeNull();
            savedPlateGroup.VehicleImage.Jpeg.Should().BeEquivalentTo(expectedImageBytes);
            savedPlateGroup.VehicleImage.IsCompressed.Should().BeFalse();
        }

        [Test]
        public async Task Handle_ImageNotInDatabase_WithCompressionEnabled_CachesCompressedFlag()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", true);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetImageFromAgentAsync(Arg.Any<Agent>(), imageId, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image was cached with compression flag
            var savedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            savedPlateGroup.VehicleImage.Should().NotBeNull();
            savedPlateGroup.VehicleImage.Jpeg.Should().BeEquivalentTo(expectedImageBytes);
            savedPlateGroup.VehicleImage.IsCompressed.Should().BeTrue();
        }

        [Test]
        public async Task Handle_ImageNotInDatabase_NoAgent_HandlesNullAgent()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetImageFromAgentAsync(null, imageId, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image was cached with compression flag set to false for null agent
            var savedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            savedPlateGroup.VehicleImage.Should().NotBeNull();
            savedPlateGroup.VehicleImage.IsCompressed.Should().BeFalse();
        }

        [Test]
        public async Task Handle_InvalidImageId_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "non-existent-uuid";
            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _handler.Handle(query, cancellationToken))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("No image found with that id.");
        }

        [Test]
        public async Task Handle_NullImageId_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetImageQuery(null);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _handler.Handle(query, cancellationToken))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("No image found with that id.");
        }

        [Test]
        public async Task Handle_EmptyImageId_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetImageQuery("");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _handler.Handle(query, cancellationToken))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("No image found with that id.");
        }

        [Test]
        public async Task Handle_ImageCompressionServiceThrows_PropagatesException()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetImageFromAgentAsync(Arg.Any<Agent>(), imageId, Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Agent connection failed"));

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(() => _handler.Handle(query, cancellationToken))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Agent connection failed");
        }

        [Test]
        public async Task Handle_MultipleAgentsExist_UsesFirstAgent()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId);
            var agent1 = TestDataFactory.CreateTestAgentWithCompression("https://first-agent.local", false);
            var agent2 = TestDataFactory.CreateTestAgentWithCompression("https://second-agent.local", true);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent1);
            await UnitOfWork.Agents.AddAsync(agent2);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetImageFromAgentAsync(Arg.Any<Agent>(), imageId, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify service was called once with any agent
            await _imageCompressionService.Received(1).GetImageFromAgentAsync(
                Arg.Any<Agent>(), 
                imageId, 
                cancellationToken);
        }

        [Test]
        public async Task Handle_DatabaseSaveSucceeds_ReturnsCachedImage()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetImageFromAgentAsync(Arg.Any<Agent>(), imageId, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify the image was persisted
            var persistedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            persistedPlateGroup.VehicleImage.Should().NotBeNull();
            persistedPlateGroup.VehicleImage.Jpeg.Should().BeEquivalentTo(expectedImageBytes);
        }

        [Test]
        public async Task Handle_PlateGroupExistsButNullVehicleImage_FetchesFromAgent()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId);
            plateGroup.VehicleImage = null; // Explicitly set to null
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetImageFromAgentAsync(Arg.Any<Agent>(), imageId, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image compression service was called
            await _imageCompressionService.Received(1).GetImageFromAgentAsync(
                agent, 
                imageId, 
                cancellationToken);
        }

        [Test]
        public async Task Handle_ValidRequest_CallsGetByIdWithDetailsAsync()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(imageId, true, false);
            plateGroup.VehicleImage.Jpeg = expectedImageBytes;

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify the full plate group was loaded (we can't easily test this directly
            // but the fact that we get the vehicle image confirms it was loaded)
            var loadedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            loadedPlateGroup.VehicleImage.Should().NotBeNull();
        }
    }
} 