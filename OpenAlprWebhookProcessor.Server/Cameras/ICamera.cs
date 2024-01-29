using OpenAlprWebhookProcessor.Cameras.ZoomAndFocus;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public interface ICamera
    {
        Task ClearCameraTextAsync(
            CancellationToken cancellationToken);

        Task SetCameraTextAsync(
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken);

        Task TriggerDayNightModeAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken);

        Task SetZoomAndFocusAsync(
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken);

        Task<bool> TriggerAutoFocusAsync(CancellationToken cancellationToken);

        Task<ZoomFocus> GetZoomAndFocusAsync(CancellationToken cancellationToken);

        Task<Stream> GetSnapshotAsync(CancellationToken cancellationToken);
    }
}
