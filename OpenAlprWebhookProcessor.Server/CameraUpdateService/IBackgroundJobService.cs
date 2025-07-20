using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface IBackgroundJobService
    {
        Task EnqueueProcessJobAsync(CameraUpdateRequest request);

        Task EnqueueProcessSunriseSunsetJobAsync(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob);

        string ScheduleClearOverlayJob(Guid cameraId, TimeSpan delay);

        Task<string> ScheduleProcessSunriseSunsetJobAsync(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob, DateTimeOffset scheduleAt);

        void DeleteJob(string jobId);
    }
} 