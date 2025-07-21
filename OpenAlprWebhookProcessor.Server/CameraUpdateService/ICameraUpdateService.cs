using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface ICameraUpdateService
    {
        Task ScheduleOverlayRequestAsync(CameraUpdateRequest cameraUpdateRequest);

        Task ScheduleDayNightTaskAsync();

        Task DeleteSunriseSunsetAsync(Guid cameraId);

        Task SetZoomAndFocusAsync(Guid cameraId, ZoomFocus zoomAndFocus, CancellationToken cancellationToken);

        Task<bool> TriggerAutofocusAsync(Guid cameraId, CancellationToken cancellationToken);

        Task<ZoomFocus> GetZoomAndFocusAsync(Guid cameraId, CancellationToken cancellationToken);

        Task EnqueueDayNightAsync(Guid cameraId, SunriseSunset sunriseSunset);

        Task ProcessSunriseSunsetJobAsync(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob);

        Task ProcessJobAsync(CameraUpdateRequest cameraUpdateRequest);

        Task ClearExpiredOverlayAsync(Guid cameraId);

        Task ForceClearOverlaysAsync(CancellationToken cancellationToken = default);
    }
}