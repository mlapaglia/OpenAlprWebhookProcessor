using NUnit.Framework;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.CameraUpdateService.Hikvision;

namespace Tests.CameraUpdateService.Hikvision
{
    [TestFixture]
    public class HikvisionCameraTests
    {
        private HttpTest _httpTest;

        private OpenAlprWebhookProcessor.Data.Camera _testCamera;

        private HikvisionCamera _hikvisionCamera;

        [SetUp]
        public void SetUp()
        {
            _httpTest = new HttpTest();

            _testCamera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                IpAddress = "192.168.1.100",
                UpdateOverlayTextUrl = "/ISAPI/System/Video/inputs/channels/1/overlays/text",
                CameraUsername = "admin",
                CameraPassword = "password123"
            };

            var clientCache = new FlurlClientCache();
            _hikvisionCamera = new HikvisionCamera(_testCamera, clientCache);
        }

        [TearDown]
        public void TearDown()
        {
            _httpTest?.Dispose();
        }

        [Test]
        public async Task ClearCameraTextAsync_SendsCorrectXmlRequest()
        {
            _httpTest.RespondWith("OK");

            await _hikvisionCamera.ClearCameraTextAsync(CancellationToken.None);

            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}{_testCamera.UpdateOverlayTextUrl}")
                .WithVerb(HttpMethod.Put)
                .Times(1);
        }

        [Test]
        public async Task SetCameraTextAsync_SendsCorrectXmlWithAllFields()
        {
            _httpTest.RespondWith("OK");
            var updateRequest = new CameraUpdateRequest
            {
                LicensePlate = "ABC123",
                VehicleDescription = "Blue Honda",
                OpenAlprProcessingTimeMs = 150,
                ProcessedPlateConfidence = 95.5
            };

            await _hikvisionCamera.SetCameraTextAsync(updateRequest, CancellationToken.None);

            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}{_testCamera.UpdateOverlayTextUrl}")
                .WithVerb(HttpMethod.Put)
                .With(call => call.RequestBody.Contains("<enabled>true</enabled>"))
                .With(call => call.RequestBody.Contains("<displayText>ABC123</displayText>"))
                .With(call => call.RequestBody.Contains("<displayText>Blue Honda</displayText>"))
                .With(call => call.RequestBody.Contains("<displayText>Processing Time: 150ms</displayText>"))
                .With(call => call.RequestBody.Contains("<displayText>Confidence: 95.5%</displayText>"))
                .Times(1);
        }

        [Test]
        public async Task TriggerDayNightModeAsync_Sunrise_SendsDayMode()
        {
            _httpTest.RespondWith("OK");

            await _hikvisionCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, CancellationToken.None);

            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}/ISAPI/Image/channels/1")
                .WithVerb(HttpMethod.Put)
                .With(call => call.RequestBody.Contains("<IrcutFilterType>day</IrcutFilterType>"))
                .Times(1);
        }

        [Test]
        public async Task TriggerDayNightModeAsync_Sunset_SendsNightMode()
        {
            _httpTest.RespondWith("OK");

            await _hikvisionCamera.TriggerDayNightModeAsync(SunriseSunset.Sunset, CancellationToken.None);

            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}/ISAPI/Image/channels/1")
                .WithVerb(HttpMethod.Put)
                .With(call => call.RequestBody.Contains("<IrcutFilterType>night</IrcutFilterType>"))
                .Times(1);
        }

        [Test]
        public async Task GetSnapshotAsync_ReturnsStream()
        {
            _httpTest.RespondWith("fake jpeg data");

            var result = await _hikvisionCamera.GetSnapshotAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}/ISAPI/Streaming/channels/1/picture")
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }

        [Test]
        public void SetZoomAndFocusAsync_ThrowsNotImplementedException()
        {
            var zoomFocus = new ZoomFocus { Zoom = 2.5m, Focus = 0.8m };

            var exception = Assert.ThrowsAsync<NotImplementedException>(
                () => _hikvisionCamera.SetZoomAndFocusAsync(zoomFocus, CancellationToken.None));
        }

        [Test]
        public void GetZoomAndFocusAsync_ThrowsNotImplementedException()
        {
            var exception = Assert.ThrowsAsync<NotImplementedException>(
                () => _hikvisionCamera.GetZoomAndFocusAsync(CancellationToken.None));
        }

        [Test]
        public void TriggerAutoFocusAsync_ThrowsNotImplementedException()
        {
            var exception = Assert.ThrowsAsync<NotImplementedException>(
                () => _hikvisionCamera.TriggerAutoFocusAsync(CancellationToken.None));
        }

        [Test]
        public void SetCameraTextAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            _httpTest.RespondWith("Server Error", 500);
            var updateRequest = new CameraUpdateRequest
            {
                LicensePlate = "ABC123",
                VehicleDescription = "Blue Honda",
                OpenAlprProcessingTimeMs = 150,
                ProcessedPlateConfidence = 95.5
            };

            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _hikvisionCamera.SetCameraTextAsync(updateRequest, CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error setting video overlay for camera {_testCamera.Id}"));
        }

        [Test]
        public void ClearCameraTextAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            _httpTest.RespondWith("Server Error", 500);

            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _hikvisionCamera.ClearCameraTextAsync(CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error setting video overlay for camera {_testCamera.Id}"));
        }

        [Test]
        public void TriggerDayNightModeAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            _httpTest.RespondWith("Server Error", 500);

            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _hikvisionCamera.TriggerDayNightModeAsync(SunriseSunset.Sunrise, CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error setting sunrise/sunset for camera {_testCamera.Id}"));
        }

        [Test]
        public void GetSnapshotAsync_ThrowsHttpRequestException_OnFlurlError()
        {
            _httpTest.RespondWith("Not Found", 404);

            var exception = Assert.ThrowsAsync<HttpRequestException>(
                () => _hikvisionCamera.GetSnapshotAsync(CancellationToken.None));

            Assert.That(exception.Message, Does.Contain($"Error getting snapshot from camera {_testCamera.Id}"));
        }

        [TestCase("ABC123", "Red Car", 100, 85.5)]
        [TestCase("XYZ789", "", 250, 92.1)]
        [TestCase("", "Blue Truck", 75, 100.0)]
        [TestCase("DEF456", "Green Van", 0, 0.0)]
        public async Task SetCameraTextAsync_HandlesVariousInputFormats(
            string licensePlate,
            string vehicleDescription,
            int processingTime,
            double confidence)
        {
            _httpTest.RespondWith("OK");
            var updateRequest = new CameraUpdateRequest
            {
                LicensePlate = licensePlate,
                VehicleDescription = vehicleDescription,
                OpenAlprProcessingTimeMs = processingTime,
                ProcessedPlateConfidence = confidence
            };

            await _hikvisionCamera.SetCameraTextAsync(updateRequest, CancellationToken.None);

            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}{_testCamera.UpdateOverlayTextUrl}")
                .WithVerb(HttpMethod.Put)
                .Times(1);
        }

        [Test]
        public async Task SetCameraTextAsync_CreatesProperXmlStructure()
        {
            _httpTest.RespondWith("OK");
            var updateRequest = new CameraUpdateRequest
            {
                LicensePlate = "TEST123",
                VehicleDescription = "Test Vehicle",
                OpenAlprProcessingTimeMs = 200,
                ProcessedPlateConfidence = 88.7
            };

            await _hikvisionCamera.SetCameraTextAsync(updateRequest, CancellationToken.None);

            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}{_testCamera.UpdateOverlayTextUrl}")
                .WithVerb(HttpMethod.Put)
                .With(call => call.RequestBody.Contains("<VideoOverlay"))
                .With(call => call.RequestBody.Contains("<alignment>customize</alignment>"))
                .With(call => call.RequestBody.Contains("<TextOverlayList>"))
                .With(call => call.RequestBody.Contains("<id>1</id>"))
                .With(call => call.RequestBody.Contains("<id>2</id>"))
                .With(call => call.RequestBody.Contains("<id>3</id>"))
                .With(call => call.RequestBody.Contains("<id>4</id>"))
                .Times(1);
        }

        [Test]
        public async Task ClearCameraTextAsync_CreatesProperXmlStructure()
        {
            _httpTest.RespondWith("OK");

            await _hikvisionCamera.ClearCameraTextAsync(CancellationToken.None);

            _httpTest.ShouldHaveCalled($"http://{_testCamera.IpAddress}{_testCamera.UpdateOverlayTextUrl}")
                .WithVerb(HttpMethod.Put)
                .With(call => call.RequestBody.Contains("<VideoOverlay"))
                .With(call => call.RequestBody.Contains("<alignment>customize</alignment>"))
                .With(call => call.RequestBody.Contains("<TextOverlayList>"))
                .Times(1);
        }
    }
}
