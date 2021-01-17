using OpenAlprWebhookProcessor.CameraUpdateService;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public static class DahuaCamera
    {
        public static async Task ClearCameraTextAsync(
             Data.Camera cameraToUpdate,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(
                    cameraToUpdate.CameraUsername,
                    cameraToUpdate.CameraPassword),
            });

            await SendUpdateCommandAsync(
                client,
                cameraToUpdate,
                "||||",
                cancellationToken);
        }

        public static async Task SetCameraTextAsync(
            Data.Camera cameraToUpdate,
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(
                    cameraToUpdate.CameraUsername,
                    cameraToUpdate.CameraPassword),
            });

            await SendUpdateCommandAsync(
                client,
                cameraToUpdate,
                $"{updateRequest.LicensePlate}|{updateRequest.VehicleDescription}|Processing Time: {updateRequest.OpenAlprProcessingTimeMs}ms|Confidence: {updateRequest.ProcessedPlateConfidence}%",
                cancellationToken);
        }

        private static async Task SendUpdateCommandAsync(
            HttpClient client,
            Data.Camera cameraToUpdate,
            string textToSet,
            CancellationToken cancellationToken)
        {
            var response = await client.PostAsync(
                $"{cameraToUpdate.UpdateOverlayTextUrl}" + textToSet,
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("unable to update video overlay: " + await response.Content.ReadAsStringAsync());
            }
        }
    }
}