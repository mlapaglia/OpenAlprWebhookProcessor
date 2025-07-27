using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface ICamera
    {
        Task ClearCameraTextAsync(
            CancellationToken cancellationToken = default);

        Task SetCameraTextAsync(
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken = default);

        Task TriggerDayNightModeAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken = default);

        Task SetZoomAndFocusAsync(
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken = default);

        Task<bool> TriggerAutoFocusAsync(CancellationToken cancellationToken = default);

        Task<ZoomFocus> GetZoomAndFocusAsync(CancellationToken cancellationToken = default);

        Task<Stream> GetSnapshotAsync(CancellationToken cancellationToken = default);
    }
}
