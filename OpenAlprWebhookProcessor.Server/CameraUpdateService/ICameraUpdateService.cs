using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface ICameraUpdateService
    {
        void ScheduleOverlayRequest(CameraUpdateRequest cameraUpdateRequest);
        Task ScheduleDayNightTaskAsync();
        Task DeleteSunriseSunsetAsync(Guid cameraId);
        Task SetZoomAndFocusAsync(Guid cameraId, ZoomFocus zoomAndFocus, CancellationToken cancellationToken);
        Task<bool> TriggerAutofocusAsync(Guid cameraId, CancellationToken cancellationToken);
        Task<ZoomFocus> GetZoomAndFocusAsync(Guid cameraId, CancellationToken cancellationToken);
        void EnqueueDayNight(Guid cameraId, SunriseSunset sunriseSunset);
    }
} 