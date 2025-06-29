using OpenAlprWebhookProcessor.Server.Cameras.ZoomAndFocus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.CameraUpdateService
{
    public interface ICameraUpdateService
    {
        Task ClearExpiredOverlayAsync(Guid cameraId);

        Task DeleteSunriseSunsetAsync(
            Guid cameraId,
            CancellationToken cancellationToken);

        void EnqueueDayNight(
            Guid cameraId,
            SunriseSunset sunriseSunset);

        Task ForceSunriseSunsetAsync(CancellationToken cancellationToken);

        Task<ZoomFocus> GetZoomAndFocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken);

        Task ProcessJobAsync(CameraUpdateRequest cameraUpdateRequest);

        Task ProcessSunriseSunsetJobAsync(
            Guid cameraId, 
            SunriseSunset sunriseSunset,
            bool scheduleNextJob);

        Task ScheduleDayNightTaskAsync(CancellationToken cancellationToken);

        void ScheduleOverlayRequest(CameraUpdateRequest cameraUpdateRequest);

        Task SetZoomAndFocusAsync(
            Guid cameraId,
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken);

        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);

        Task<bool> TriggerAutofocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken);
    }
}