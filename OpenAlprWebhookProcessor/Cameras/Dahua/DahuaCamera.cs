using OpenAlprWebhookProcessor.CameraUpdateService;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class DahuaCamera : ICamera
    {
        private readonly Data.Camera _camera;

        private readonly HttpClient _httpClient;

        public DahuaCamera(Data.Camera camera)
        {

            _camera = camera;
            _httpClient = GetHttpClient();
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
            var response = await _httpClient.PostAsync(
                $"{_camera.UpdateOverlayTextUrl}" + textToSet,
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("unable to update video overlay: " + await response.Content.ReadAsStringAsync(cancellationToken));
            }
        }

        private async Task SendDayNightCommandAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsync(
                $"{_camera.UpdateDayNightModeUrl}{(sunriseSunset == SunriseSunset.Sunrise ? 0 : 1)}",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("unable to set sunrise/sunset: " + await response.Content.ReadAsStringAsync(cancellationToken));
            }
        }

        private HttpClient GetHttpClient()
        {
            return new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(
                _camera.CameraUsername,
                _camera.CameraPassword),
            });
        }
    }
}