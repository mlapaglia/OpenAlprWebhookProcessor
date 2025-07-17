using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public interface ICameraScheduling
    {
        void ExecuteSingleDayNightTask(
            SunriseSunset sunriseSunset,
            Guid cameraId,
            IBackgroundJobService backgroundJobService);

        Task ScheduleDayNightTasksAsync(
            IBackgroundJobService backgroundJobService);

        void ScheduleDayNightTask(
            IBackgroundJobService backgroundJobService,
            Agent agent,
            Data.Camera camera);

        bool IsSunUp(double latitude, double longitude);
    }
} 