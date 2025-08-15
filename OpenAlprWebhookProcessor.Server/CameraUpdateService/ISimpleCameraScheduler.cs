using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface ISimpleCameraScheduler
    {
        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);

        Task ResetAllCamerasAsync(CancellationToken cancellationToken = default);

        Task ScheduleCameraAsync(Guid cameraId, CancellationToken cancellationToken = default);

        Task RemoveCameraScheduleAsync(Guid cameraId, CancellationToken cancellationToken = default);

        Task RescheduleAllCamerasAsync(CancellationToken cancellationToken = default);

        DateTimeOffset? GetNextScheduledExecutionTime(Guid cameraId);

        List<ScheduledJobInfo> GetAllScheduledJobs();

        Task ScheduleOverlayAsync(CameraUpdateRequest cameraUpdateRequest, CancellationToken cancellationToken = default);

        Task ClearOverlayAsync(Guid cameraId, CancellationToken cancellationToken = default);

        Task ExecuteDayNightModeAsync(Guid cameraId, SunriseSunset sunriseSunset, CancellationToken cancellationToken = default);

        Task SetZoomAndFocusAsync(Guid cameraId, ZoomFocus zoomAndFocus, CancellationToken cancellationToken = default);

        Task<ZoomFocus> GetZoomAndFocusAsync(Guid cameraId, CancellationToken cancellationToken = default);

        Task<bool> TriggerAutofocusAsync(Guid cameraId, CancellationToken cancellationToken = default);
    }
}
