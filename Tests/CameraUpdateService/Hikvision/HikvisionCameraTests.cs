using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.CameraUpdateService.Hikvision;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService.Hikvision
{
    [TestFixture]
    public class HikvisionCameraTests : TestBase
    {
        private HikvisionCamera _hikvisionCamera;
        private TestHttpMessageHandler _testHttpHandler;
        private IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _testHttpHandler = new TestHttpMessageHandler();
            _httpClient = new HttpClient(_testHttpHandler);
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            
            // Setup the factory to return our test HTTP client
            _httpClientFactory.CreateClient().Returns(_httpClient);
            
            var testCamera = CreateTestDataCameraForHikvision();
            _hikvisionCamera = new HikvisionCamera(testCamera, _httpClientFactory);
        }

        [TearDown]
        public override void TearDown()
        {
            _httpClient?.Dispose();
            _testHttpHandler?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Arrange
            var camera = CreateTestDataCameraForHikvision();
            var httpClientFactory = Substitute.For<IHttpClientFactory>();

            // Act
            var hikvisionCamera = new HikvisionCamera(camera, httpClientFactory);

            // Assert
            hikvisionCamera.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithNullCamera_DoesNotThrowImmediately()
        {
            // Arrange
            var httpClientFactory = Substitute.For<IHttpClientFactory>();

            // Act
            var hikvisionCamera = new HikvisionCamera(null, httpClientFactory);

            // Assert
            hikvisionCamera.Should().NotBeNull();
            // Note: The null camera will cause issues when methods are called,
            // but the constructor itself doesn't validate parameters
        }

        [Test]
        public void Constructor_WithNullHttpClientFactory_DoesNotThrowImmediately()
        {
            // Arrange
            var camera = CreateTestDataCameraForHikvision();

            // Act
            var hikvisionCamera = new HikvisionCamera(camera, null);

            // Assert
            hikvisionCamera.Should().NotBeNull();
            // Note: The null httpClientFactory will cause issues when methods are called,
            // but the constructor itself doesn't validate parameters
        }

        [Test]
        public async Task ClearCameraTextAsync_WithValidRequest_SendsCorrectXmlRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var cancellationToken = GetCancellationToken();

            // Act
            await _hikvisionCamera.ClearCameraTextAsync(cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Put);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/test-overlay-url");
            
            var content = _testHttpHandler.RequestContent;
            content.Should().NotBeEmpty();
            
            // Verify XML structure
            var videoOverlay = DeserializeVideoOverlay(content);
            videoOverlay.Should().NotBeNull();
            videoOverlay.Alignment.Should().Be("customize");
            videoOverlay.TextOverlayList.TextOverlay.Should().HaveCount(4);
            
            // All overlays should be disabled and empty
            foreach (var overlay in videoOverlay.TextOverlayList.TextOverlay)
            {
                overlay.Enabled.Should().Be("false");
                overlay.DisplayText.Should().BeEmpty();
            }
        }

        [Test]
        public async Task SetCameraTextAsync_WithValidRequest_SendsCorrectXmlRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var updateRequest = CreateTestCameraUpdateRequest();
            var cancellationToken = GetCancellationToken();

            // Act
            await _hikvisionCamera.SetCameraTextAsync(updateRequest, cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Put);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/test-overlay-url");
            
            var content = _testHttpHandler.RequestContent;
            content.Should().NotBeEmpty();
            
            // Verify XML structure and content
            var videoOverlay = DeserializeVideoOverlay(content);
            videoOverlay.Should().NotBeNull();
            videoOverlay.Alignment.Should().Be("customize");
            videoOverlay.TextOverlayList.TextOverlay.Should().HaveCount(4);
            
            // Verify specific overlay content
            videoOverlay.TextOverlayList.TextOverlay[0].Id.Should().Be("1");
            videoOverlay.TextOverlayList.TextOverlay[0].Enabled.Should().Be("true");
            videoOverlay.TextOverlayList.TextOverlay[0].DisplayText.Should().Be("ABC123");
            
            videoOverlay.TextOverlayList.TextOverlay[1].Id.Should().Be("2");
            videoOverlay.TextOverlayList.TextOverlay[1].Enabled.Should().Be("true");
            videoOverlay.TextOverlayList.TextOverlay[1].DisplayText.Should().Be("Toyota Camry");
            
            videoOverlay.TextOverlayList.TextOverlay[2].Id.Should().Be("3");
            videoOverlay.TextOverlayList.TextOverlay[2].Enabled.Should().Be("true");
            videoOverlay.TextOverlayList.TextOverlay[2].DisplayText.Should().Be("Processing Time: 150.5ms");
            
            videoOverlay.TextOverlayList.TextOverlay[3].Id.Should().Be("4");
            videoOverlay.TextOverlayList.TextOverlay[3].Enabled.Should().Be("true");
            videoOverlay.TextOverlayList.TextOverlay[3].DisplayText.Should().Be("Confidence: 95.8%");
        }

        [Test]
        public async Task SetCameraTextAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.BadRequest, "Error occurred");
            var updateRequest = CreateTestCameraUpdateRequest();
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            try
            {
                await _hikvisionCamera.SetCameraTextAsync(updateRequest, cancellationToken);
                Assert.Fail("Expected HttpRequestException was not thrown");
            }
            catch (HttpRequestException ex)
            {
                ex.Message.Should().StartWith("Error setting video overlay for camera");
                ex.Message.Should().Contain("Error occurred");
            }
        }

        [Test]
        public async Task TriggerDayNightModeAsync_WithSunrise_SendsCorrectRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var cancellationToken = GetCancellationToken();

            // Act
            await _hikvisionCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Put);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/ISAPI/Image/channels/1");
            
            var content = _testHttpHandler.RequestContent;
            content.Should().Contain("<IrcutFilterType>day</IrcutFilterType>");
            content.Should().Contain("http://www.hikvision.com/ver20/XMLSchema");
        }

        [Test]
        public async Task TriggerDayNightModeAsync_WithSunset_SendsCorrectRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var cancellationToken = GetCancellationToken();

            // Act
            await _hikvisionCamera.TriggerDayNightModeAsync(SunriseSunset.Sunset, cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Put);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/ISAPI/Image/channels/1");
            
            var content = _testHttpHandler.RequestContent;
            content.Should().Contain("<IrcutFilterType>night</IrcutFilterType>");
            content.Should().Contain("http://www.hikvision.com/ver20/XMLSchema");
        }

        [Test]
        public async Task TriggerDayNightModeAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.InternalServerError, "Camera error");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            try
            {
                await _hikvisionCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, cancellationToken);
                Assert.Fail("Expected HttpRequestException was not thrown");
            }
            catch (HttpRequestException ex)
            {
                ex.Message.Should().StartWith("Error setting sunrise/sunset for camera");
                ex.Message.Should().Contain("Camera error");
            }
        }

        [Test]
        public async Task GetSnapshotAsync_WithSuccessResponse_ReturnsStream()
        {
            // Arrange
            var imageBytes = Encoding.UTF8.GetBytes("fake image data");
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, imageBytes);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _hikvisionCamera.GetSnapshotAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Get);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/ISAPI/Streaming/channels/1/picture");
            
            // Verify stream content
            using var reader = new StreamReader(result);
            var content = await reader.ReadToEndAsync();
            content.Should().Be("fake image data");
        }

        [Test]
        public async Task GetSnapshotAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.NotFound, "Not found");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            try
            {
                await _hikvisionCamera.GetSnapshotAsync(cancellationToken);
                Assert.Fail("Expected HttpRequestException was not thrown");
            }
            catch (HttpRequestException ex)
            {
                ex.Message.Should().StartWith("Error getting snapshot from camera");
                ex.Message.Should().Contain("Not found");
            }
        }

        [Test]
        public void SetZoomAndFocusAsync_NotImplemented_ThrowsNotImplementedException()
        {
            // Arrange
            var zoomFocus = new ZoomFocus { Zoom = 1.5m, Focus = 2.0m };
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(() =>
                _hikvisionCamera.SetZoomAndFocusAsync(zoomFocus, cancellationToken));
        }

        [Test]
        public void GetZoomAndFocusAsync_NotImplemented_ThrowsNotImplementedException()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(() =>
                _hikvisionCamera.GetZoomAndFocusAsync(cancellationToken));
        }

        [Test]
        public void TriggerAutoFocusAsync_NotImplemented_ThrowsNotImplementedException()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(() =>
                _hikvisionCamera.TriggerAutoFocusAsync(cancellationToken));
        }

        [Test]
        public async Task ClearCameraTextAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            _testHttpHandler.SetupDelayedResponse(HttpStatusCode.OK, "Success", TimeSpan.FromSeconds(1));
            
            // Cancel immediately
            await cancellationTokenSource.CancelAsync();

            // Act & Assert
            try
            {
                await _hikvisionCamera.ClearCameraTextAsync(cancellationTokenSource.Token);
                Assert.Fail("Expected OperationCanceledException was not thrown");
            }
            catch (OperationCanceledException)
            {
                // Expected exception - test passes
            }
        }

        private static OpenAlprWebhookProcessor.Data.Camera CreateTestDataCameraForHikvision()
        {
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            camera.IpAddress = "192.168.1.100";
            camera.UpdateOverlayTextUrl = "http://192.168.1.100/test-overlay-url";
            camera.CameraUsername = "testuser";
            camera.CameraPassword = "testpass";
            return camera;
        }

        private static CameraUpdateRequest CreateTestCameraUpdateRequest()
        {
            return new CameraUpdateRequest
            {
                Id = Guid.NewGuid(),
                LicensePlate = "ABC123",
                VehicleDescription = "Toyota Camry",
                OpenAlprProcessingTimeMs = 150.5,
                ProcessedPlateConfidence = 95.8,
                LicensePlateImageUuid = Guid.NewGuid().ToString(),
                IsAlert = false,
                IsTest = false,
                IsSinglePlate = false,
                IsPreviewGroup = false
            };
        }

        private static VideoOverlay DeserializeVideoOverlay(string xmlContent)
        {
            var serializer = new XmlSerializer(typeof(VideoOverlay));
            using var reader = new StringReader(xmlContent);
            return (VideoOverlay)serializer.Deserialize(reader);
        }
    }
} 