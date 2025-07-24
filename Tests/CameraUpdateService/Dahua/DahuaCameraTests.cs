using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using System.Net;
using System.Text;
using Tests.TestHelpers;

namespace Tests.CameraUpdateService.Dahua
{
    [TestFixture]
    public class DahuaCameraTests : TestBase
    {
        private DahuaCamera _dahuaCamera;
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
            
            var testCamera = CreateTestDataCameraForDahua();
            _dahuaCamera = new DahuaCamera(testCamera, _httpClientFactory);
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
            var camera = CreateTestDataCameraForDahua();
            var httpClientFactory = Substitute.For<IHttpClientFactory>();

            // Act
            var dahuaCamera = new DahuaCamera(camera, httpClientFactory);

            // Assert
            dahuaCamera.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithNullCamera_DoesNotThrowImmediately()
        {
            // Arrange
            var httpClientFactory = Substitute.For<IHttpClientFactory>();

            // Act
            var dahuaCamera = new DahuaCamera(null, httpClientFactory);

            // Assert
            dahuaCamera.Should().NotBeNull();
            // Note: The null camera will cause issues when methods are called,
            // but the constructor itself doesn't validate parameters
        }

        [Test]
        public void Constructor_WithNullHttpClientFactory_DoesNotThrowImmediately()
        {
            // Arrange
            var camera = CreateTestDataCameraForDahua();

            // Act
            var dahuaCamera = new DahuaCamera(camera, null);

            // Assert
            dahuaCamera.Should().NotBeNull();
            // Note: The null httpClientFactory will cause issues when methods are called,
            // but the constructor itself doesn't validate parameters
        }

        [Test]
        public async Task ClearCameraTextAsync_WithValidRequest_SendsCorrectPostRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var cancellationToken = GetCancellationToken();

            // Act
            await _dahuaCamera.ClearCameraTextAsync(cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Post);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/test-overlay-url||||");
            
            // Verify that Basic auth is set
            _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Basic");
        }

        [Test]
        public async Task SetCameraTextAsync_WithValidRequest_SendsCorrectPostRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var updateRequest = CreateTestCameraUpdateRequest();
            var cancellationToken = GetCancellationToken();

            // Act
            await _dahuaCamera.SetCameraTextAsync(updateRequest, cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Post);
            _testHttpHandler.RequestUri.ToString().Should()
                .Be("http://192.168.1.100/test-overlay-urlABC123|Toyota Camry|Processing Time: 150.5ms|Confidence: 95.8%25");
            
            // Verify that Basic auth is set
            _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Basic");
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
                await _dahuaCamera.SetCameraTextAsync(updateRequest, cancellationToken);
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
            await _dahuaCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Post);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/test-day-night-url0");
            
            // Verify that Basic auth is set
            _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Basic");
        }

        [Test]
        public async Task TriggerDayNightModeAsync_WithSunset_SendsCorrectRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var cancellationToken = GetCancellationToken();

            // Act
            await _dahuaCamera.TriggerDayNightModeAsync(SunriseSunset.Sunset, cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Post);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/test-day-night-url1");
            
            // Verify that Basic auth is set
            _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Basic");
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
                await _dahuaCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, cancellationToken);
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
            var result = await _dahuaCamera.GetSnapshotAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Get);
            _testHttpHandler.RequestUri.ToString().Should().Be("http://192.168.1.100/cgi-bin/snapshot.cgi");
            
            // Verify stream content
            using var reader = new StreamReader(result);
            var content = await reader.ReadToEndAsync();
            content.Should().Be("fake image data");
            
            // Verify that Basic auth is set
            _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Basic");
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
                await _dahuaCamera.GetSnapshotAsync(cancellationToken);
                Assert.Fail("Expected HttpRequestException was not thrown");
            }
            catch (HttpRequestException ex)
            {
                ex.Message.Should().StartWith("Error getting snapshot from camera");
                ex.Message.Should().Contain("Not found");
            }
        }

        [Test]
        public async Task SetZoomAndFocusAsync_WithValidZoomFocus_SendsCorrectPostRequest()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, "Success");
            var zoomFocus = new ZoomFocus { Zoom = 1.5m, Focus = 2.0m };
            var cancellationToken = GetCancellationToken();

            // Act
            await _dahuaCamera.SetZoomAndFocusAsync(zoomFocus, cancellationToken);

            // Assert
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Post);
            _testHttpHandler.RequestUri.ToString().Should()
                .Be("http://192.168.1.100/cgi-bin/devVideoInput.cgi?action=adjustFocus&focus=2.0&zoom=1.5");
            
            // Verify that Basic auth is set
            _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Basic");
        }

        [Test]
        public async Task SetZoomAndFocusAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.BadRequest, "Focus error");
            var zoomFocus = new ZoomFocus { Zoom = 1.5m, Focus = 2.0m };
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            try
            {
                await _dahuaCamera.SetZoomAndFocusAsync(zoomFocus, cancellationToken);
                Assert.Fail("Expected HttpRequestException was not thrown");
            }
            catch (HttpRequestException ex)
            {
                ex.Message.Should().StartWith("Error setting zoom and focus for camera");
                ex.Message.Should().Contain("Focus error");
            }
        }

        [Test]
        public async Task GetZoomAndFocusAsync_WithValidResponse_ParsesCorrectly()
        {
            // Arrange
            var mockResponse = "status.Focus=2.5\r\nstatus.Zoom=1.8\r\n";
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, mockResponse);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _dahuaCamera.GetZoomAndFocusAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Focus.Should().Be(2.5m);
            result.Zoom.Should().Be(1.8m);
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Post);
            _testHttpHandler.RequestUri.ToString().Should()
                .Be("http://192.168.1.100/cgi-bin/devVideoInput.cgi?action=getFocusStatus");
        }

        [Test]
        public async Task GetZoomAndFocusAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.BadRequest, "Get focus error");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            try
            {
                await _dahuaCamera.GetZoomAndFocusAsync(cancellationToken);
                Assert.Fail("Expected HttpRequestException was not thrown");
            }
            catch (HttpRequestException ex)
            {
                ex.Message.Should().StartWith("Error getting zoom and focus from camera");
            }
        }

        [Test]
        public async Task TriggerAutoFocusAsync_WithValidResponse_ReturnsTrue()
        {
            // Arrange
            var mockResponse = "result\":true\"";
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, mockResponse);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _dahuaCamera.TriggerAutoFocusAsync(cancellationToken);

            // Assert
            result.Should().BeTrue();
            _testHttpHandler.RequestMethod.Should().Be(HttpMethod.Post);
            _testHttpHandler.RequestUri.ToString().Should()
                .Be("http://192.168.1.100/cgi-bin/devVideoInput.cgi?action=autoFocus");
        }

        [Test]
        public async Task TriggerAutoFocusAsync_WithFalseResponse_ReturnsFalse()
        {
            // Arrange
            var mockResponse = "result\":false\"";
            _testHttpHandler.SetupResponse(HttpStatusCode.OK, mockResponse);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _dahuaCamera.TriggerAutoFocusAsync(cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task TriggerAutoFocusAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            _testHttpHandler.SetupResponse(HttpStatusCode.BadRequest, "Auto focus error");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            try
            {
                await _dahuaCamera.TriggerAutoFocusAsync(cancellationToken);
                Assert.Fail("Expected HttpRequestException was not thrown");
            }
            catch (HttpRequestException ex)
            {
                ex.Message.Should().StartWith("Error triggering auto focus for camera");
                ex.Message.Should().Contain("Auto focus error");
            }
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
                await _dahuaCamera.ClearCameraTextAsync(cancellationTokenSource.Token);
                Assert.Fail("Expected OperationCanceledException was not thrown");
            }
            catch (OperationCanceledException)
            {
                // Expected exception - test passes
            }
        }

        [Test]
        public async Task SetCameraTextAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var updateRequest = CreateTestCameraUpdateRequest();
            _testHttpHandler.SetupDelayedResponse(HttpStatusCode.OK, "Success", TimeSpan.FromSeconds(1));
            
            // Cancel immediately
            await cancellationTokenSource.CancelAsync();

            // Act & Assert
            try
            {
                await _dahuaCamera.SetCameraTextAsync(updateRequest, cancellationTokenSource.Token);
                Assert.Fail("Expected OperationCanceledException was not thrown");
            }
            catch (OperationCanceledException)
            {
                // Expected exception - test passes
            }
        }

        private static OpenAlprWebhookProcessor.Data.Camera CreateTestDataCameraForDahua()
        {
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua);
            camera.IpAddress = "192.168.1.100";
            camera.UpdateOverlayTextUrl = "http://192.168.1.100/test-overlay-url";
            camera.UpdateDayNightModeUrl = "http://192.168.1.100/test-day-night-url";
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
    }
} 