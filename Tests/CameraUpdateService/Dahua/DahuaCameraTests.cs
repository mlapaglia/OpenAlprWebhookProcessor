using NUnit.Framework;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace Tests.CameraUpdateService.Dahua
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class DahuaCameraTests
    {
        private HttpTest _httpTest;
        private OpenAlprWebhookProcessor.Data.Camera _testCamera;
        private DahuaCamera _dahuaCamera;

        [SetUp]
        public void SetUp()
        {
            // Flurl's HttpTest intercepts all HTTP calls globally
            _httpTest = new HttpTest();

            _testCamera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                IpAddress = "192.168.1.100",
                UpdateOverlayTextUrl = "cgi-bin/configManager.cgi?action=setConfig&VideoWidget[0].CustomTitle[1].Text=",
                UpdateDayNightModeUrl = "cgi-bin/configManager.cgi?action=setConfig&VideoInOptions[0].NightOptions.SwitchMode=",
                CameraUsername = "admin",
                CameraPassword = "password123"
            };

            // Use the default Flurl client cache - HttpTest will intercept the calls
            var clientCache = new FlurlClientCache();
            _dahuaCamera = new DahuaCamera(_testCamera, clientCache);
        }

        [TearDown]
        public void TearDown()
        {
            _httpTest?.Dispose();
        }

        [Test]
        public async Task ClearCameraTextAsync_SendsCorrectRequest()
        {
            // Arrange
            _httpTest.RespondWith("OK");

            // Act
            await _dahuaCamera.ClearCameraTextAsync(CancellationToken.None);

            // Assert with URL-encoded pipe characters (| becomes %7C)
            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}/{_testCamera.UpdateOverlayTextUrl}%7C%7C%7C%7C")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public async Task SetCameraTextAsync_FormatsTextCorrectly()
        {
            // Arrange
            _httpTest.RespondWith("OK");
            var updateRequest = new CameraUpdateRequest
            {
                LicensePlate = "ABC123",
                VehicleDescription = "Blue Honda",
                OpenAlprProcessingTimeMs = 150,
                ProcessedPlateConfidence = 95.5
            };

            // Act
            await _dahuaCamera.SetCameraTextAsync(updateRequest, CancellationToken.None);

            // Assert
            _httpTest.ShouldHaveCalled("*ABC123*")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public async Task TriggerDayNightModeAsync_Sunrise_SendsZero()
        {
            // Arrange
            _httpTest.RespondWith("OK");

            // Act
            await _dahuaCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, CancellationToken.None);

            // Assert
            var expectedUrl = $"http://{_testCamera.IpAddress}/{_testCamera.UpdateDayNightModeUrl}0";
            _httpTest.ShouldHaveCalled(expectedUrl)
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public async Task TriggerDayNightModeAsync_Sunset_SendsOne()
        {
            // Arrange
            _httpTest.RespondWith("OK");

            // Act
            await _dahuaCamera.TriggerDayNightModeAsync(SunriseSunset.Sunset, CancellationToken.None);

            // Assert
            var expectedUrl = $"http://{_testCamera.IpAddress}/{_testCamera.UpdateDayNightModeUrl}1";
            _httpTest.ShouldHaveCalled(expectedUrl)
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public async Task SetZoomAndFocusAsync_SendsCorrectParameters()
        {
            // Arrange
            _httpTest.RespondWith("OK");
            var zoomFocus = new ZoomFocus { Zoom = 2.5m, Focus = 0.8m };

            // Act
            await _dahuaCamera.SetZoomAndFocusAsync(zoomFocus, CancellationToken.None);

            // Assert
            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}/cgi-bin/devVideoInput.cgi")
                .WithQueryParam("action", "adjustFocus")
                .WithQueryParam("focus", "0.8")
                .WithQueryParam("zoom", "2.5")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public async Task GetZoomAndFocusAsync_ParsesResponseCorrectly()
        {
            // Arrange
            var responseText = "status.Focus=0.75\r\nstatus.Zoom=3.2\r\nother=data";
            _httpTest.RespondWith(responseText);

            // Act
            var result = await _dahuaCamera.GetZoomAndFocusAsync(CancellationToken.None);

            // Assert
            Assert.That(result.Focus, Is.EqualTo(0.75m));
            Assert.That(result.Zoom, Is.EqualTo(3.2m));
            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}/cgi-bin/devVideoInput.cgi")
                .WithQueryParam("action", "getFocusStatus")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public async Task TriggerAutoFocusAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var responseText = "result\":true\"";
            _httpTest.RespondWith(responseText);

            // Act
            var result = await _dahuaCamera.TriggerAutoFocusAsync(CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}/cgi-bin/devVideoInput.cgi")
                .WithQueryParam("action", "autoFocus")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public async Task TriggerAutoFocusAsync_ReturnsFalse_WhenUnsuccessful()
        {
            // Arrange
            var responseText = "result\":false\"";
            _httpTest.RespondWith(responseText);

            // Act
            var result = await _dahuaCamera.TriggerAutoFocusAsync(CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SetCameraTextAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            // Arrange
            _httpTest.RespondWith("Server Error", 500);
            var updateRequest = new CameraUpdateRequest
            {
                LicensePlate = "ABC123",
                VehicleDescription = "Blue Honda",
                OpenAlprProcessingTimeMs = 150,
                ProcessedPlateConfidence = 95.5
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.SetCameraTextAsync(updateRequest, CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error setting video overlay for camera {_testCamera.Id}"));
        }

        [Test]
        public void GetSnapshotAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            // Arrange
            _httpTest.RespondWith("Not Found", 404);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.GetSnapshotAsync(CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error getting snapshot from camera {_testCamera.Id}"));
        }

        [Test]
        public void SendDayNightCommandAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            // Arrange
            _httpTest.RespondWith("Server Error", 500);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error setting sunrise/sunset for camera {_testCamera.Id}"));
        }

        [Test]
        public void SetZoomAndFocusAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            // Arrange
            _httpTest.RespondWith("Server Error", 500);
            var zoomFocus = new ZoomFocus { Zoom = 2.5m, Focus = 0.8m };

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.SetZoomAndFocusAsync(zoomFocus, CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error setting zoom and focus for camera {_testCamera.Id}"));
        }

        [Test]
        public void GetZoomAndFocusAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            // Arrange
            _httpTest.RespondWith("Server Error", 500);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.GetZoomAndFocusAsync(CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error getting zoom and focus from camera {_testCamera.Id}"));
        }

        [Test]
        public void TriggerAutoFocusAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            // Arrange
            _httpTest.RespondWith("Server Error", 500);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.TriggerAutoFocusAsync(CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error triggering auto focus for camera {_testCamera.Id}"));
        }

        [TestCase("ABC123", "Red Car", 100, 85.5, "ABC123|Red Car|Processing Time: 100ms|Confidence: 85.5%")]
        [TestCase("XYZ789", "", 250, 92.1, "XYZ789||Processing Time: 250ms|Confidence: 92.1%")]
        [TestCase("", "Blue Truck", 75, 100.0, "|Blue Truck|Processing Time: 75ms|Confidence: 100%")]
        public async Task SetCameraTextAsync_HandlesVariousInputFormats(
            string licensePlate,
            string vehicleDescription,
            int processingTime,
            double confidence,
            string expectedText)
        {
            // Arrange
            _httpTest.RespondWith("OK");
            var updateRequest = new CameraUpdateRequest
            {
                LicensePlate = licensePlate,
                VehicleDescription = vehicleDescription,
                OpenAlprProcessingTimeMs = processingTime,
                ProcessedPlateConfidence = confidence
            };

            // Act
            await _dahuaCamera.SetCameraTextAsync(updateRequest, CancellationToken.None);

            // Assert
            _httpTest.ShouldHaveCalled($"*{licensePlate}*")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Test]
        public void GetZoomAndFocusAsync_ThrowsException_WhenRegexDoesNotMatch()
        {
            // Arrange
            var responseText = "invalid response format";
            _httpTest.RespondWith(responseText);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.GetZoomAndFocusAsync(CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error getting zoom and focus from camera {_testCamera.Id}"));
        }

        [Test]
        public void TriggerAutoFocusAsync_ThrowsException_WhenRegexDoesNotMatch()
        {
            // Arrange
            var responseText = "invalid response format";
            _httpTest.RespondWith(responseText);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _dahuaCamera.TriggerAutoFocusAsync(CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error triggering auto focus for camera {_testCamera.Id}"));
        }
    }
}