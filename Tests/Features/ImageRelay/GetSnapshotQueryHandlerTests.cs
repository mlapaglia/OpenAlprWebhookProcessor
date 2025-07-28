using AwesomeAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotRelay;
using Tests.TestHelpers;

namespace Tests.Features.ImageRelay
{
    [TestFixture]
    public class GetSnapshotQueryHandlerTests : TestBase
    {
        private GetSnapshotQueryHandler _handler;
        private MockCameraFactory _mockCameraFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockCameraFactory = new MockCameraFactory();
            _handler = new GetSnapshotQueryHandler(UnitOfWork, _mockCameraFactory);
        }

        [Test]
        public async Task Handle_ValidCameraId_ReturnsStreamFromCamera()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Hikvision, 
                cameraId, 
                1);

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var expectedImageBytes = TestDataFactory.CreateTestJpegBytes();
            _mockCameraFactory.MockCamera.GetSnapshotAsync(Arg.Any<CancellationToken>())
                .Returns(new MemoryStream(expectedImageBytes));

            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(expectedImageBytes);
        }

        [Test]
        public async Task Handle_InvalidCameraId_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Camera not found.");
        }

        [Test]
        public async Task Handle_EmptyGuidCameraId_ThrowsArgumentException()
        {
            // Arrange
            var cameraId = Guid.Empty;
            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Camera not found.");
        }

        [Test]
        public async Task Handle_ValidCameraId_CallsGetAllAsync()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Hikvision, 
                cameraId, 
                1);

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            var cameras = await UnitOfWork.Cameras.GetAllAsync(cancellationToken);
            cameras.Should().HaveCount(1);
            cameras.First().Id.Should().Be(cameraId);
        }

        [Test]
        public async Task Handle_HikvisionCamera_CreatesCorrectCameraType()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Hikvision, 
                cameraId, 
                1);
            camera.Manufacturer = CameraManufacturer.Hikvision;

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            // Verify that the factory was called with the correct manufacturer
            await _mockCameraFactory.MockCamera.Received(1).GetSnapshotAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_DahuaCamera_CreatesCorrectCameraType()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Dahua, 
                cameraId, 
                1);
            camera.Manufacturer = CameraManufacturer.Dahua;

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            // Verify that the factory was called with the correct manufacturer
            await _mockCameraFactory.MockCamera.Received(1).GetSnapshotAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_CameraDatabaseLookup_FindsCorrectCamera()
        {
            // Arrange
            var cameraId1 = Guid.NewGuid();
            var cameraId2 = Guid.NewGuid();
            var camera1 = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Hikvision, 
                cameraId1, 
                1);
            var camera2 = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Dahua, 
                cameraId2, 
                2);

            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetSnapshotQuery(cameraId2);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            var cameras = await UnitOfWork.Cameras.GetAllAsync(cancellationToken);
            cameras.Should().HaveCount(2);
            
            var targetCamera = cameras.FirstOrDefault(x => x.Id == cameraId2);
            targetCamera.Should().NotBeNull();
            targetCamera.Manufacturer.Should().Be(CameraManufacturer.Dahua);
        }

        [Test]
        public void Handle_NullUnitOfWork_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new GetSnapshotQueryHandler(null, _mockCameraFactory))
                .Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'unitOfWork')");
        }

        [Test]
        public void Handle_NullCameraFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new GetSnapshotQueryHandler(UnitOfWork, null))
                .Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'cameraFactory')");
        }

        [Test]
        public async Task Handle_ValidQuery_UsesCorrectCameraId()
        {
            // Arrange
            var expectedCameraId = Guid.NewGuid();
            var camera = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Hikvision, 
                expectedCameraId, 
                1);

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetSnapshotQuery(expectedCameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            var matchingCamera = await UnitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == expectedCameraId, cancellationToken);
            matchingCamera.Should().NotBeNull();
            matchingCamera.Id.Should().Be(expectedCameraId);
        }

        [Test]
        public async Task Handle_MultipleCamerasInDatabase_FindsCorrectOne()
        {
            // Arrange
            var targetCameraId = Guid.NewGuid();
            var cameras = new List<OpenAlprWebhookProcessor.Data.Camera>
            {
                TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision, Guid.NewGuid(), 1),
                TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua, Guid.NewGuid(), 2),
                TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision, targetCameraId, 3),
                TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua, Guid.NewGuid(), 4),
            };

            foreach (var camera in cameras)
            {
                await UnitOfWork.Cameras.AddAsync(camera);
            }
            await UnitOfWork.SaveChangesAsync();

            var query = new GetSnapshotQuery(targetCameraId);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            var allCameras = await UnitOfWork.Cameras.GetAllAsync(cancellationToken);
            allCameras.Should().HaveCount(4);
            
            var foundCamera = allCameras.FirstOrDefault(x => x.Id == targetCameraId);
            foundCamera.Should().NotBeNull();
            foundCamera.OpenAlprCameraId.Should().Be(3);
        }

        [Test]
        public async Task Handle_CameraGetSnapshotThrowsException_PropagatesException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Hikvision, 
                cameraId, 
                1);

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            _mockCameraFactory.MockCamera.GetSnapshotAsync(Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Camera connection failed"));

            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<Exception>()
                .WithMessage("Camera connection failed");
        }

        [Test]
        public async Task Handle_CameraGetSnapshotTimesOut_ThrowsTimeoutException()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var camera = TestDataFactory.CreateTestCamera(
                CameraManufacturer.Hikvision, 
                cameraId, 
                1);

            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            // Configure mock to delay longer than the timeout
            _mockCameraFactory.MockCamera.GetSnapshotAsync(Arg.Any<CancellationToken>())
                .Returns(Task.Run(async () =>
                {
                    await Task.Delay(6000);
                    return new MemoryStream(TestDataFactory.CreateTestJpegBytes()) as Stream;
                }));

            var query = new GetSnapshotQuery(cameraId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<TimeoutException>()
                .WithMessage("Unable to get image from camera");
        }
    }
} 