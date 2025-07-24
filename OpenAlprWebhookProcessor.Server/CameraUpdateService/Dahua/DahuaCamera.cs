using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Utilities;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public partial class DahuaCamera : ICamera
    {
        private readonly Data.Camera _camera;

        private readonly IHttpClientFactory _httpClientFactory;

        public DahuaCamera(Data.Camera camera, IHttpClientFactory httpClientFactory)
        {
            _camera = camera;
            _httpClientFactory = httpClientFactory;
        }

        public async Task ClearCameraTextAsync(
            CancellationToken cancellationToken)
        {
            await SendUpdateCommandAsync(
                "||||",
                cancellationToken);
        }

        public async Task SetCameraTextAsync(
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken)
        {
            await SendUpdateCommandAsync(
                $"{updateRequest.LicensePlate}|{updateRequest.VehicleDescription}|Processing Time: {updateRequest.OpenAlprProcessingTimeMs}ms|Confidence: {updateRequest.ProcessedPlateConfidence}%",
                cancellationToken);
        }

        public async Task TriggerDayNightModeAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken)
        {
            await SendDayNightCommandAsync(
                sunriseSunset,
                cancellationToken);
        }

        private async Task SendUpdateCommandAsync(
            string textToSet,
            CancellationToken cancellationToken)
        {
            using var httpClient = GetConfiguredHttpClient();
            var result = await httpClient.PostAsync(
                $"{_camera.UpdateOverlayTextUrl}" + textToSet,
                null,
                cancellationToken);

            await result.EnsureSuccessWithDetailsAsync($"Error setting video overlay for camera {_camera.Id}", cancellationToken);
        }

        private async Task SendDayNightCommandAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken)
        {
            using var httpClient = GetConfiguredHttpClient();
            var result = await httpClient.PostAsync(
                $"{_camera.UpdateDayNightModeUrl}{(sunriseSunset == SunriseSunset.Sunrise ? 0 : 1)}",
                null,
                cancellationToken);

            await result.EnsureSuccessWithDetailsAsync($"Error setting sunrise/sunset for camera {_camera.Id}", cancellationToken);
        }

        private HttpClient GetConfiguredHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            if (!string.IsNullOrEmpty(_camera.CameraUsername) && !string.IsNullOrEmpty(_camera.CameraPassword))
            {
                var authValue = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_camera.CameraUsername}:{_camera.CameraPassword}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            }

            return httpClient;
        }

        public async Task<Stream> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            using var httpClient = GetConfiguredHttpClient();
            var result = await httpClient.GetAsync(
                $"http://{_camera.IpAddress}/cgi-bin/snapshot.cgi",
                cancellationToken);

            await result.EnsureSuccessWithDetailsAsync($"Error getting snapshot from camera {_camera.Id}", cancellationToken);

            return await result.Content.ReadAsStreamAsync(cancellationToken);
        }

        public async Task SetZoomAndFocusAsync(
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken)
        {
            using var httpClient = GetConfiguredHttpClient();
            var result = await httpClient.PostAsync(
                $"http://{_camera.IpAddress}/cgi-bin/devVideoInput.cgi?action=adjustFocus&focus={zoomAndFocus.Focus}&zoom={zoomAndFocus.Zoom}",
                null,
                cancellationToken);

            await result.EnsureSuccessWithDetailsAsync($"Error setting zoom and focus for camera {_camera.Id}", cancellationToken);
        }

        public async Task<ZoomFocus> GetZoomAndFocusAsync(CancellationToken cancellationToken)
        {
            using var httpClient = GetConfiguredHttpClient();
            var result = await httpClient.PostAsync(
                $"http://{_camera.IpAddress}/cgi-bin/devVideoInput.cgi?action=getFocusStatus",
                null,
                cancellationToken);

            await result.EnsureSuccessWithDetailsAsync($"Error getting zoom and focus from camera {_camera.Id}", cancellationToken);

            var response = await result.Content.ReadAsStringAsync(cancellationToken);

            return new ZoomFocus()
            {
                Focus = decimal.Parse(FocusRegex().Match(response).Groups[1].Value),
                Zoom = decimal.Parse(ZoomRegex().Match(response).Groups[1].Value),
            };
        }

        public async Task<bool> TriggerAutoFocusAsync(CancellationToken cancellationToken)
        {
            using var httpClient = GetConfiguredHttpClient();
            var result = await httpClient.PostAsync(
                $"http://{_camera.IpAddress}/cgi-bin/devVideoInput.cgi?action=autoFocus",
                null,
                cancellationToken);

            await result.EnsureSuccessWithDetailsAsync($"Error triggering auto focus for camera {_camera.Id}", cancellationToken);

            var response = await result.Content.ReadAsStringAsync(cancellationToken);

            return bool.Parse(SuccessRegex().Match(response).Groups[1].Value);
        }

        [GeneratedRegex("result\":(.*?)\"")]
        private static partial Regex SuccessRegex();

        [GeneratedRegex("status\\.Focus=(.*)\\r")]
        private static partial Regex FocusRegex();

        [GeneratedRegex("status\\.Zoom=(.*)\\r")]
        private static partial Regex ZoomRegex();
    }
}