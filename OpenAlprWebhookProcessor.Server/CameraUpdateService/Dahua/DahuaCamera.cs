using Flurl.Http;
using Flurl.Http.Configuration;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public partial class DahuaCamera : ICamera
    {
        private readonly Data.Camera _camera;

        private readonly IFlurlClientCache _flurlClientCache;

        public DahuaCamera(Data.Camera camera, IFlurlClientCache flurlClientCache)
        {
            _camera = camera;
            _flurlClientCache = flurlClientCache;
        }

        public async Task ClearCameraTextAsync(
            CancellationToken cancellationToken = default)
        {
            await SendUpdateCommandAsync(
                "||||",
                cancellationToken);
        }

        public async Task SetCameraTextAsync(
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken = default)
        {
            await SendUpdateCommandAsync(
                $"{updateRequest.LicensePlate}|{updateRequest.VehicleDescription}|Processing Time: {updateRequest.OpenAlprProcessingTimeMs}ms|Confidence: {updateRequest.ProcessedPlateConfidence}%",
                cancellationToken);
        }

        public async Task TriggerDayNightModeAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken = default)
        {
            await SendDayNightCommandAsync(
                sunriseSunset,
                cancellationToken);
        }

        private async Task SendUpdateCommandAsync(
            string textToSet,
            CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request(_camera.UpdateOverlayTextUrl + textToSet)
                    .PostAsync(null, cancellationToken: cancellationToken);

                response.ResponseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error setting video overlay for camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        private async Task SendDayNightCommandAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request($"{_camera.UpdateDayNightModeUrl}{(sunriseSunset == SunriseSunset.Sunrise ? 0 : 1)}")
                    .PostAsync(null, cancellationToken: cancellationToken);

                response.ResponseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error setting sunrise/sunset for camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        private IFlurlClient GetConfiguredFlurlClient()
        {
            return _flurlClientCache.GetOrAdd($"camera*{_camera.Id}", $"http://{_camera.IpAddress}", (fluentClientBuilder) =>
            {
                if (!string.IsNullOrEmpty(_camera.CameraUsername) && !string.IsNullOrEmpty(_camera.CameraPassword))
                {
                    fluentClientBuilder.ConfigureInnerHandler(handler =>
                    {
                        handler.UseDefaultCredentials = true;
                        handler.Credentials = new NetworkCredential(
                            _camera.CameraUsername,
                            _camera.CameraPassword);
                    });
                }
            });
        }

        public async Task<Stream> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request($"cgi-bin/snapshot.cgi")
                    .GetAsync(cancellationToken: cancellationToken);

                return await response.GetStreamAsync();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error getting snapshot from camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        public async Task SetZoomAndFocusAsync(
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request($"cgi-bin/devVideoInput.cgi")
                    .SetQueryParam("action", "adjustFocus")
                    .SetQueryParam("focus", zoomAndFocus.Focus)
                    .SetQueryParam("zoom", zoomAndFocus.Zoom)
                    .PostAsync(null, cancellationToken: cancellationToken);

                response.ResponseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error setting zoom and focus for camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        public async Task<ZoomFocus> GetZoomAndFocusAsync(CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request($"cgi-bin/devVideoInput.cgi")
                    .SetQueryParam("action", "getFocusStatus")
                    .PostAsync(null, cancellationToken: cancellationToken);

                var responseText = await response.GetStringAsync();

                return new ZoomFocus()
                {
                    Focus = decimal.Parse(FocusRegex().Match(responseText).Groups[1].Value),
                    Zoom = decimal.Parse(ZoomRegex().Match(responseText).Groups[1].Value),
                };
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error getting zoom and focus from camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> TriggerAutoFocusAsync(CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request($"cgi-bin/devVideoInput.cgi")
                    .SetQueryParam("action", "autoFocus")
                    .PostAsync(null, cancellationToken: cancellationToken);

                var responseText = await response.GetStringAsync();

                return bool.Parse(SuccessRegex().Match(responseText).Groups[1].Value);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error triggering auto focus for camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        [GeneratedRegex("result\":(.*?)\"")]
        private static partial Regex SuccessRegex();

        [GeneratedRegex("status\\.Focus=(.*)\\r")]
        private static partial Regex FocusRegex();

        [GeneratedRegex("status\\.Zoom=(.*)\\r")]
        private static partial Regex ZoomRegex();
    }
}