using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public interface ICamera
    {
        Task SetCameraTextAsync(
            int openAlprCameraId,
            string plateNumber,
            string vehicleDescription,
            CancellationToken cancellationToken);

        Task ClearCameraTextAsync(
            int openAlprCameraId,
            CancellationToken cancellationToken);
    }
}
