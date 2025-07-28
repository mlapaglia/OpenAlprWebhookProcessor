using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameras;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetPlateCaptures;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus;
using System.Text.Json;
using Tests.TestHelpers;

namespace Tests.Features.Cameras.Queries
{
    [TestFixture]
    public class GetCamerasQueryHandlerTests : TestBase
    {
        private IBackgroundJobService _backgroundJobService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _backgroundJobService = Substitute.For<IBackgroundJobService>();
        }

        [Test]
        public async Task GetCamerasQueryHandler_WithCameras_ReturnsAllCameras()
        {
            // Arrange
            var handler = new GetCamerasQueryHandler(UnitOfWork, _backgroundJobService);
            var agent = TestDataFactory.CreateTestAgent();
            var camera1 = TestDataFactory.CreateTestCamera("Camera 1", 1);
            var camera2 = TestDataFactory.CreateTestCamera("Camera 2", 2);
            
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera1);
            await UnitOfWork.Cameras.AddAsync(camera2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCamerasQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.OpenAlprName == "Camera 1");
            result.Should().Contain(c => c.OpenAlprName == "Camera 2");
        }

        [Test]
        public async Task GetCamerasQueryHandler_NoCameras_ReturnsEmptyList()
        {
            // Arrange
            var handler = new GetCamerasQueryHandler(UnitOfWork, _backgroundJobService);
            var agent = TestDataFactory.CreateTestAgent();
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCamerasQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetCamerasQueryHandler_WithScheduledJob_ReturnsNullScheduledInfo()
        {
            // Arrange
            var handler = new GetCamerasQueryHandler(UnitOfWork, _backgroundJobService);
            var agent = TestDataFactory.CreateTestAgent();
            var camera = TestDataFactory.CreateTestCamera("Camera 1", 1);
            camera.NextDayNightScheduleId = "job-123";
            
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCamerasQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            // With the new timer-based system, scheduled job info is not tracked
            result.First().DayNightNextScheduledCommand.Should().BeNull();
        }

        [Test]
        public async Task GetCamerasQueryHandler_WithoutScheduledJob_ReturnsNullScheduledInfo()
        {
            // Arrange
            var handler = new GetCamerasQueryHandler(UnitOfWork, _backgroundJobService);
            var agent = TestDataFactory.CreateTestAgent();
            var camera = TestDataFactory.CreateTestCamera("Camera 1", 1);
            camera.NextDayNightScheduleId = null;
            
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCamerasQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().DayNightNextScheduledCommand.Should().BeNull();
        }

        [Test]
        public async Task GetCamerasQueryHandler_WithLatestPlateUuid_ReturnsSampleImageUrl()
        {
            // Arrange
            var handler = new GetCamerasQueryHandler(UnitOfWork, _backgroundJobService);
            var agent = TestDataFactory.CreateTestAgent();
            var camera = TestDataFactory.CreateTestCamera("Camera 1", 1);
            camera.UpdateOverlayEnabled = true;
            camera.LatestProcessedPlateUuid = "plate-123";
            
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCamerasQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().SampleImageUrl.Should().Contain("plate-123");
        }

        [Test]
        public async Task GetCamerasQueryHandler_WithoutLatestPlateUuid_ReturnsSnapshotUrl()
        {
            // Arrange
            var handler = new GetCamerasQueryHandler(UnitOfWork, _backgroundJobService);
            var agent = TestDataFactory.CreateTestAgent();
            var camera = TestDataFactory.CreateTestCamera("Camera 1", 1);
            camera.UpdateOverlayEnabled = true;
            camera.LatestProcessedPlateUuid = null;
            
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCamerasQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().SampleImageUrl.Should().BeNull();
        }
    }

    [TestFixture]
    public class GetCameraMaskQueryHandlerTests : TestBase
    {
        [Test]
        public async Task GetCameraMaskQueryHandler_WithMask_ReturnsCoordinates()
        {
            // Arrange
            var handler = new GetCameraMaskQueryHandler(UnitOfWork);
            var agent = TestDataFactory.CreateTestAgent();
            var camera = TestDataFactory.CreateTestCamera("Test Camera", 1);
            var cameraId = camera.Id;
            var coordinates = new List<MaskCoordinate>
            {
                new MaskCoordinate { X = 10, Y = 20 },
                new MaskCoordinate { X = 30, Y = 40 }
            };

            var cameraMask = new OpenAlprWebhookProcessor.Data.CameraMask
            {
                Id = Guid.NewGuid(),
                CameraId = cameraId,
                Coordinates = JsonSerializer.Serialize(coordinates)
            };

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.CameraMasks.AddAsync(cameraMask);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCameraMaskQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.X == 10 && c.Y == 20);
            result.Should().Contain(c => c.X == 30 && c.Y == 40);
        }

        [Test]
        public async Task GetCameraMaskQueryHandler_WithoutMask_ReturnsEmptyList()
        {
            // Arrange
            var handler = new GetCameraMaskQueryHandler(UnitOfWork);
            var cameraId = Guid.NewGuid();
            var query = new GetCameraMaskQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetCameraMaskQueryHandler_WithNullCoordinates_ReturnsEmptyList()
        {
            // Arrange
            var handler = new GetCameraMaskQueryHandler(UnitOfWork);
            var agent = TestDataFactory.CreateTestAgent();
            var camera = TestDataFactory.CreateTestCamera("Test Camera", 1);
            var cameraId = camera.Id;

            var cameraMask = new OpenAlprWebhookProcessor.Data.CameraMask
            {
                Id = Guid.NewGuid(),
                CameraId = cameraId,
                Coordinates = null
            };

            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.CameraMasks.AddAsync(cameraMask);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetCameraMaskQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetCameraMaskQueryHandler_NonExistentCamera_ReturnsEmptyList()
        {
            // Arrange
            var handler = new GetCameraMaskQueryHandler(UnitOfWork);
            var cameraId = Guid.NewGuid();
            var query = new GetCameraMaskQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }

    [TestFixture]
    public class GetPlateCapturesQueryHandlerTests : TestBase
    {
        [Test]
        public async Task GetPlateCapturesQueryHandler_WithPlateCaptures_ReturnsImageUrls()
        {
            // Arrange
            var handler = new GetPlateCapturesQueryHandler(UnitOfWork);
            var cameraId = Guid.NewGuid();
            var openAlprCameraId = 42;
            
            var camera = TestDataFactory.CreateTestCamera("Test Camera", openAlprCameraId);
            camera.Id = cameraId;
            await UnitOfWork.Cameras.AddAsync(camera);

            // Create test plate groups with plate images
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup("ABC123");
            plateGroup1.OpenAlprCameraId = openAlprCameraId;
            plateGroup1.OpenAlprUuid = "uuid-1";
            plateGroup1.PlateImage = new OpenAlprWebhookProcessor.Data.PlateImage
            {
                Id = Guid.NewGuid(),
                PlateGroupId = plateGroup1.Id
            };

            var plateGroup2 = TestDataFactory.CreateTestPlateGroup("XYZ789");
            plateGroup2.OpenAlprCameraId = openAlprCameraId;
            plateGroup2.OpenAlprUuid = "uuid-2";
            plateGroup2.PlateImage = new OpenAlprWebhookProcessor.Data.PlateImage
            {
                Id = Guid.NewGuid(),
                PlateGroupId = plateGroup2.Id
            };

            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetPlateCapturesQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(url => url.Contains("uuid-1"));
            result.Should().Contain(url => url.Contains("uuid-2"));
        }

        [Test]
        public async Task GetPlateCapturesQueryHandler_WithoutPlateImages_ReturnsEmptyList()
        {
            // Arrange
            var handler = new GetPlateCapturesQueryHandler(UnitOfWork);
            var cameraId = Guid.NewGuid();
            var openAlprCameraId = 42;
            
            var camera = TestDataFactory.CreateTestCamera("Test Camera", openAlprCameraId);
            camera.Id = cameraId;
            await UnitOfWork.Cameras.AddAsync(camera);

            // Create test plate groups without plate images
            var plateGroup = TestDataFactory.CreateTestPlateGroup("ABC123");
            plateGroup.OpenAlprCameraId = openAlprCameraId;
            plateGroup.OpenAlprUuid = "uuid-1";
            plateGroup.PlateImage = null;

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetPlateCapturesQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetPlateCapturesQueryHandler_NonExistentCamera_ReturnsEmptyList()
        {
            // Arrange
            var handler = new GetPlateCapturesQueryHandler(UnitOfWork);
            var cameraId = Guid.NewGuid();
            var query = new GetPlateCapturesQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetPlateCapturesQueryHandler_LimitsToTen_ReturnsOnlyTenResults()
        {
            // Arrange
            var handler = new GetPlateCapturesQueryHandler(UnitOfWork);
            var cameraId = Guid.NewGuid();
            var openAlprCameraId = 42;
            
            var camera = TestDataFactory.CreateTestCamera("Test Camera", openAlprCameraId);
            camera.Id = cameraId;
            await UnitOfWork.Cameras.AddAsync(camera);

            // Create 15 plate groups with plate images
            for (int i = 0; i < 15; i++)
            {
                var plateGroup = TestDataFactory.CreateTestPlateGroup($"ABC{i:D3}");
                plateGroup.OpenAlprCameraId = openAlprCameraId;
                plateGroup.OpenAlprUuid = $"uuid-{i}";
                plateGroup.PlateImage = new OpenAlprWebhookProcessor.Data.PlateImage
                {
                    Id = Guid.NewGuid(),
                    PlateGroupId = plateGroup.Id
                };
                await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            }

            await UnitOfWork.SaveChangesAsync();

            var query = new GetPlateCapturesQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(10);
        }
    }

    [TestFixture]
    public class GetZoomAndFocusQueryHandlerTests : TestBase
    {
        private ICameraUpdateService _cameraUpdateService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _cameraUpdateService = Substitute.For<ICameraUpdateService>();
        }

        [Test]
        public async Task GetZoomAndFocusQueryHandler_ValidCommand_CallsGetZoomAndFocus()
        {
            // Arrange
            var handler = new GetZoomAndFocusQueryHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            var expectedZoomFocus = new ZoomFocus { Focus = 50, Zoom = 75 };
            
            _cameraUpdateService.GetZoomAndFocusAsync(cameraId, Arg.Any<CancellationToken>())
                .Returns(expectedZoomFocus);

            var query = new GetZoomAndFocusQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Focus.Should().Be(50);
            result.Zoom.Should().Be(75);
            await _cameraUpdateService.Received(1).GetZoomAndFocusAsync(cameraId, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetZoomAndFocusQueryHandler_ServiceReturnsNull_ReturnsNull()
        {
            // Arrange
            var handler = new GetZoomAndFocusQueryHandler(_cameraUpdateService);
            var cameraId = Guid.NewGuid();
            
            _cameraUpdateService.GetZoomAndFocusAsync(cameraId, Arg.Any<CancellationToken>())
                .Returns((ZoomFocus)null);

            var query = new GetZoomAndFocusQuery(cameraId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
} 