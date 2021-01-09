using OpenAlprWebhookProcessor.Cameras.Configuration;
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
            CameraConfiguration cameraToUpdate,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(
                    cameraToUpdate.Username,
                    cameraToUpdate.Password),
            });

            await SendUpdateCommandAsync(
                client,
                cameraToUpdate,
                1,
                "||||",
                cancellationToken);
        }

        public static async Task SetCameraTextAsync(
            CameraConfiguration cameraToUpdate,
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(
                    cameraToUpdate.Username,
                    cameraToUpdate.Password),
            });

            await SendUpdateCommandAsync(
                client,
                cameraToUpdate,
                1,
                $"{updateRequest.LicensePlate}|{updateRequest.VehicleDescription}|Processing Time: {updateRequest.OpenAlprProcessingTimeMs}ms|Confidence: {updateRequest.ProcessedPlateConfidence}%",
                cancellationToken);
        }

        private static async Task SendUpdateCommandAsync(
            HttpClient client,
            CameraConfiguration cameraToUpdate,
            int textFieldId,
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