using CoordinateSharp;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public static class CameraScheduling
    {
        public static void ExecuteSingleDayNightTask(
            SunriseSunset sunriseSunset,
            Guid cameraId,
            IBackgroundJobService backgroundJobService)
        {
            ArgumentNullException.ThrowIfNull(backgroundJobService);

            backgroundJobService.EnqueueProcessSunriseSunsetJob(
                cameraId,
                sunriseSunset,
                false);
        }

        public static async Task ScheduleDayNightTasksAsync(
            IUnitOfWork unitOfWork,
            IBackgroundJobService backgroundJobService,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(backgroundJobService);

            var camerasToUpdate = await unitOfWork.Cameras.FindAsync(x => x.UpdateDayNightModeEnabled, cancellationToken);

            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            foreach (var camera in camerasToUpdate)
            {
                ScheduleDayNightTask(
                    backgroundJobService,
                    agent,
                    camera);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public static void ScheduleDayNightTask(
            IBackgroundJobService backgroundJobService,
            Agent agent,
            Data.Camera camera)
        {
            ArgumentNullException.ThrowIfNull(backgroundJobService);
            ArgumentNullException.ThrowIfNull(agent);
            ArgumentNullException.ThrowIfNull(camera);

            var timeZoneOffset = camera.TimezoneOffset ?? agent.TimeZoneOffset;
            var latitude = camera.Latitude ?? agent.Latitude;
            var longitude = camera.Longitude ?? agent.Longitude;
            var sunriseOffset = camera.SunriseOffset ?? agent.SunriseOffset;
            var sunsetOffset = camera.SunsetOffset ?? agent.SunsetOffset;

            var nextSunrise = Celestial.Get_Next_SunRise(
                latitude.Value,
                longitude.Value,
                DateTime.Now,
                timeZoneOffset);

            var nextSunset = Celestial.Get_Next_SunSet(
                latitude.Value,
                longitude.Value,
                DateTime.Now,
                timeZoneOffset);

            var isSunUp = Celestial.CalculateCelestialTimes(
                latitude.Value,
                longitude.Value,
                DateTime.Now,
                timeZoneOffset).IsSunUp;

            var cameraSunriseAt = nextSunrise.AddMinutes(sunriseOffset);
            var cameraSunsetAt = nextSunset.AddMinutes(sunsetOffset);

            if (!string.IsNullOrWhiteSpace(camera.NextDayNightScheduleId))
            {
                backgroundJobService.DeleteJob(camera.NextDayNightScheduleId);
            }

            camera.NextDayNightScheduleId = backgroundJobService.ScheduleProcessSunriseSunsetJob(
                camera.Id,
                isSunUp ? SunriseSunset.Sunset : SunriseSunset.Sunrise,
                true,
                isSunUp ? cameraSunsetAt : cameraSunriseAt);
        }

        public static bool IsSunUp(
            double latitude,
            double longitude)
        {
            var cameraCoordinate = new Coordinate(
                latitude,
                longitude,
                DateTime.UtcNow);

            return cameraCoordinate.CelestialInfo.IsSunUp;
        }
    }
}
