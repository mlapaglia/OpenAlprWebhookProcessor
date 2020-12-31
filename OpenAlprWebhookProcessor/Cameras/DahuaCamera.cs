using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class DahuaCamera : ICamera
    {
        public Task ClearCameraTextAsync(
            int openAlprCameraId,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetCameraTextAsync(
            int openAlprCameraId,
            string plateNumber,
            string vehicleDescription,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
