using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface ICameraUpdateService
    {
        Task ScheduleOverlayRequestAsync(
            CameraUpdateRequest cameraUpdateRequest,
            CancellationToken cancellationToken = default);

        Task ScheduleDayNightTaskAsync(CancellationToken cancellationToken = default);

        Task DeleteSunriseSunsetAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default);

        Task SetZoomAndFocusAsync(
            Guid cameraId,
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken = default);

        Task<bool> TriggerAutofocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default);

        Task<ZoomFocus> GetZoomAndFocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default);

        Task EnqueueDayNightAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken = default);

        Task ProcessSunriseSunsetJobAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            bool scheduleNextJob,
            CancellationToken cancellationToken = default);

        Task ProcessJobAsync(
            CameraUpdateRequest cameraUpdateRequest,
            CancellationToken cancellationToken = default);

        Task ClearExpiredOverlayAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default);

        Task ForceClearOverlaysAsync(CancellationToken cancellationToken = default);
    }
}