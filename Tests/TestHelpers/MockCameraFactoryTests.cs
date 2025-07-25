using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;

namespace Tests.TestHelpers
{
    [TestFixture]
    public class MockCameraFactoryTests
    {
        private MockCameraFactory _mockCameraFactory;
        private OpenAlprWebhookProcessor.Data.Camera _testCamera;

        [SetUp]
        public void SetUp()
        {
            _mockCameraFactory = new MockCameraFactory();
            _testCamera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                IpAddress = "192.168.1.100",
                CameraUsername = "admin",
                CameraPassword = "password123"
            };
        }

        [Test]
        public void Constructor_ShouldInitializeMockCamera()
        {
            var factory = new MockCameraFactory();

            factory.MockCamera.Should().NotBeNull();
        }

        [Test]
        public void Create_WithHikvisionManufacturer_ShouldReturnMockCamera()
        {
            var result = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);

            result.Should().BeSameAs(_mockCameraFactory.MockCamera);
        }

        [Test]
        public void Create_WithDahuaManufacturer_ShouldReturnMockCamera()
        {
            var result = _mockCameraFactory.Create(CameraManufacturer.Dahua, _testCamera);

            result.Should().BeSameAs(_mockCameraFactory.MockCamera);
        }

        [Test]
        public void Create_WithAnyManufacturer_ShouldReturnSameMockInstance()
        {
            var hikvisionCamera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);
            var dahuaCamera = _mockCameraFactory.Create(CameraManufacturer.Dahua, _testCamera);

            hikvisionCamera.Should().BeSameAs(dahuaCamera);
            hikvisionCamera.Should().BeSameAs(_mockCameraFactory.MockCamera);
        }

        [Test]
        public async Task MockCamera_GetSnapshotAsync_ShouldReturnConfiguredStream()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);

            var result = await camera.GetSnapshotAsync(CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeOfType<MemoryStream>();
            
            var memoryStream = result as MemoryStream;
            memoryStream.ToArray().Should().BeEquivalentTo(TestDataFactory.CreateTestJpegBytes());
        }

        [Test]
        public async Task MockCamera_SetCameraTextAsync_ShouldBeCallable()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);
            var updateRequest = new CameraUpdateRequest
            {
                Id = _testCamera.Id,
                LicensePlate = "TEST123",
                VehicleDescription = "Test Vehicle"
            };

            var act = async () => await camera.SetCameraTextAsync(updateRequest, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task MockCamera_ClearCameraTextAsync_ShouldBeCallable()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);

            var act = async () => await camera.ClearCameraTextAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task MockCamera_TriggerDayNightModeAsync_ShouldBeCallable()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);

            var act = async () => await camera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task MockCamera_SetZoomAndFocusAsync_ShouldBeCallable()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);
            var zoomFocus = new ZoomFocus { Zoom = 1.5m, Focus = 2.0m };

            var act = async () => await camera.SetZoomAndFocusAsync(zoomFocus, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task MockCamera_GetZoomAndFocusAsync_ShouldBeCallable()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);

            var act = async () => await camera.GetZoomAndFocusAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task MockCamera_TriggerAutoFocusAsync_ShouldBeCallable()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);

            var act = async () => await camera.TriggerAutoFocusAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public void MockCamera_Property_ShouldReturnSameInstanceAsCreate()
        {
            var createdCamera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);

            _mockCameraFactory.MockCamera.Should().BeSameAs(createdCamera);
        }

        [Test]
        public void MockCamera_CanConfigureSpecificBehavior()
        {
            var customStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            _mockCameraFactory.MockCamera.GetSnapshotAsync(Arg.Any<CancellationToken>())
                .Returns(customStream);

            var camera = _mockCameraFactory.Create(CameraManufacturer.Dahua, _testCamera);

            var result = camera.GetSnapshotAsync(CancellationToken.None);

            result.Result.Should().BeSameAs(customStream);
        }

        [Test]
        public void MockCamera_CanVerifyMethodCalls()
        {
            var camera = _mockCameraFactory.Create(CameraManufacturer.Hikvision, _testCamera);
            var updateRequest = new CameraUpdateRequest { Id = _testCamera.Id };

            camera.SetCameraTextAsync(updateRequest, CancellationToken.None);

            _mockCameraFactory.MockCamera.Received(1)
                .SetCameraTextAsync(updateRequest, CancellationToken.None);
        }

        [Test]
        public void Create_WithNullCamera_ShouldNotThrow()
        {
            var act = () => _mockCameraFactory.Create(CameraManufacturer.Hikvision, null);

            act.Should().NotThrow();
        }
    }
} 