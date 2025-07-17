using Hangfire;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public BackgroundJobService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
        }

        public void EnqueueProcessJob(CameraUpdateRequest request)
        {
            _backgroundJobClient.Enqueue<ICameraUpdateService>(service => service.ProcessJobAsync(request));
        }

        public void EnqueueProcessSunriseSunsetJob(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob)
        {
            _backgroundJobClient.Enqueue<ICameraUpdateService>(service => service.ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob));
        }

        public string ScheduleClearOverlayJob(Guid cameraId, TimeSpan delay)
        {
            return _backgroundJobClient.Schedule<ICameraUpdateService>(
                service => service.ClearExpiredOverlayAsync(cameraId),
                delay);
        }

        public string ScheduleProcessSunriseSunsetJob(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob, DateTimeOffset scheduleAt)
        {
            return _backgroundJobClient.Schedule<ICameraUpdateService>(
                service => service.ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob),
                scheduleAt);
        }

        public void DeleteJob(string jobId)
        {
            _backgroundJobClient.Delete(jobId);
        }
    }
} 