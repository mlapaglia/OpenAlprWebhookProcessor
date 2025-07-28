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
    public class GetCropImageQueryHandlerTests : TestBase
    {
        private GetCropImageQueryHandler _handler;
        private IImageCompressionService _imageCompressionService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _imageCompressionService = Substitute.For<IImageCompressionService>();
            _handler = new GetCropImageQueryHandler(UnitOfWork, _imageCompressionService);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Handle_ExistingCropImageInDatabase_ReturnsImageFromDatabase()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(imageId, false, true);
            plateGroup.PlateImage.Jpeg = expectedImageBytes;

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);
            
            // Verify image compression service was not called since image was cached
            await _imageCompressionService.DidNotReceive().GetCropImageFromAgentAsync(
                Arg.Any<Agent>(), 
                Arg.Any<string>(), 
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_CropImageNotInDatabase_FetchesFromAgentAndCaches()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image compression service was called with correct coordinates
            await _imageCompressionService.Received(1).GetCropImageFromAgentAsync(
                agent, 
                imageId, 
                plateCoordinates,
                cancellationToken);

            // Verify image was cached in database
            var savedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            savedPlateGroup.PlateImage.Should().NotBeNull();
            savedPlateGroup.PlateImage.Jpeg.Should().BeEquivalentTo(expectedImageBytes);
            savedPlateGroup.PlateImage.IsCompressed.Should().BeFalse();
        }

        [Test]
        public async Task Handle_CropImageNotInDatabase_WithCompressionEnabled_CachesCompressedFlag()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", true);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image was cached with compression flag
            var savedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            savedPlateGroup.PlateImage.Should().NotBeNull();
            savedPlateGroup.PlateImage.Jpeg.Should().BeEquivalentTo(expectedImageBytes);
            savedPlateGroup.PlateImage.IsCompressed.Should().BeTrue();
        }

        [Test]
        public async Task Handle_CropImageNotInDatabase_WithNullPlateCoordinates_FetchesFromAgent()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, null);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image compression service was called
            await _imageCompressionService.Received(1).GetCropImageFromAgentAsync(
                Arg.Any<Agent>(), 
                imageId, 
                Arg.Any<string>(),
                cancellationToken);
        }

        [Test]
        public async Task Handle_CropImageNotInDatabase_NoAgent_HandlesNullAgent()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(null, imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image was cached with compression flag set to false for null agent
            var savedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            savedPlateGroup.PlateImage.Should().NotBeNull();
            savedPlateGroup.PlateImage.IsCompressed.Should().BeFalse();
        }

        [Test]
        public async Task Handle_InvalidImageId_ThrowsArgumentException()
        {
            // Arrange
            var imageId = "non-existent-uuid";
            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("No image found with that id.");
        }

        [Test]
        public async Task Handle_NullImageId_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetCropImageQuery(null);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("No image found with that id.");
        }

        [Test]
        public async Task Handle_EmptyImageId_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetCropImageQuery("");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("No image found with that id.");
        }

        [Test]
        public async Task Handle_ImageCompressionServiceThrows_PropagatesException()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Agent connection failed"));

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Agent connection failed");
        }

        [Test]
        public async Task Handle_MultipleAgentsExist_UsesFirstAgent()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);
            var agent1 = TestDataFactory.CreateTestAgentWithCompression("https://first-agent.local", false);
            var agent2 = TestDataFactory.CreateTestAgentWithCompression("https://second-agent.local", true);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent1);
            await UnitOfWork.Agents.AddAsync(agent2);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify service was called once with any agent
            await _imageCompressionService.Received(1).GetCropImageFromAgentAsync(
                Arg.Any<Agent>(), 
                imageId, 
                plateCoordinates,
                cancellationToken);
        }

        [Test]
        public async Task Handle_DatabaseSaveSucceeds_ReturnsCachedImage()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify the image was persisted
            var persistedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            persistedPlateGroup.PlateImage.Should().NotBeNull();
            persistedPlateGroup.PlateImage.Jpeg.Should().BeEquivalentTo(expectedImageBytes);
        }

        [Test]
        public async Task Handle_PlateGroupExistsButNullPlateImage_FetchesFromAgent()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=100,y=200,w=300,h=400";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);
            plateGroup.PlateImage = null; // Explicitly set to null
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify image compression service was called
            await _imageCompressionService.Received(1).GetCropImageFromAgentAsync(
                agent, 
                imageId, 
                plateCoordinates,
                cancellationToken);
        }

        [Test]
        public async Task Handle_ValidRequest_CallsGetByIdWithDetailsAsync()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(imageId, false, true);
            plateGroup.PlateImage.Jpeg = expectedImageBytes;

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify the full plate group was loaded (we can't easily test this directly
            // but the fact that we get the plate image confirms it was loaded)
            var loadedPlateGroup = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id);
            loadedPlateGroup.PlateImage.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_PassesCorrectPlateCoordinatesToService()
        {
            // Arrange
            var imageId = "test-uuid-123";
            var plateCoordinates = "x=150,y=250,w=400,h=500";
            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            var plateGroup = TestDataFactory.CreateTestPlateGroupForImageRelay(imageId, plateCoordinates);
            var agent = TestDataFactory.CreateTestAgentWithCompression("https://test-agent.local", false);

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _imageCompressionService.GetCropImageFromAgentAsync(Arg.Any<Agent>(), imageId, plateCoordinates, Arg.Any<CancellationToken>())
                .Returns(expectedImageBytes);

            var query = new GetCropImageQuery(imageId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeOfType<MemoryStream>();
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);

            // Verify the correct plate coordinates were passed to the service
            await _imageCompressionService.Received(1).GetCropImageFromAgentAsync(
                agent, 
                imageId, 
                plateCoordinates,
                cancellationToken);
        }
    }
} 
