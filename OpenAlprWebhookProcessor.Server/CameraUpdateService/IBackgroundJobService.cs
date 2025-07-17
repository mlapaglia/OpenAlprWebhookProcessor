using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface IBackgroundJobService
    {
        void EnqueueProcessJob(CameraUpdateRequest request);
        void EnqueueProcessSunriseSunsetJob(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob);
        string ScheduleClearOverlayJob(Guid cameraId, TimeSpan delay);
        string ScheduleProcessSunriseSunsetJob(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob, DateTimeOffset scheduleAt);
        void DeleteJob(string jobId);
    }
} 