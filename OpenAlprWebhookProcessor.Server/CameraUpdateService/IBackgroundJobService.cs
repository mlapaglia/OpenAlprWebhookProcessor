using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface IBackgroundJobService
    {
        Task EnqueueProcessJobAsync(
            CameraUpdateRequest request,
            CancellationToken cancellationToken = default);

        Task EnqueueProcessSunriseSunsetJobAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            bool scheduleNextJob,
            CancellationToken cancellationToken = default);

        string ScheduleClearOverlayJob(
            Guid cameraId,
            TimeSpan delay);

        string ScheduleProcessSunriseSunsetJob(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            bool scheduleNextJob,
            DateTimeOffset scheduleAt);

        void DeleteJob(string jobId);

        DateTimeOffset? GetNextScheduledExecutionTime(string jobId);
    }
} 