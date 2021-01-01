using OpenAlprWebhookProcessor.Cameras.Configuration;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public static class DahuaCamera
    {
        public static async Task ClearCameraTextAsync(
            Camera cameraToUpdate,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static async Task SetCameraTextAsync(
            Camera cameraToUpdate,
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
