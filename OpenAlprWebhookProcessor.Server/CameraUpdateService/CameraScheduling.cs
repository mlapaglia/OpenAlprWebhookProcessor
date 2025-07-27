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
        public static async Task ExecuteSingleDayNightTaskAsync(
            SunriseSunset sunriseSunset,
            Guid cameraId,
            IBackgroundJobService backgroundJobService,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(backgroundJobService);

            await backgroundJobService.EnqueueProcessSunriseSunsetJobAsync(
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
                await ScheduleDayNightTaskAsync(
                    backgroundJobService,
                    agent,
                    camera,
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public static async Task ScheduleDayNightTaskAsync(
            IBackgroundJobService backgroundJobService,
            Agent agent,
            Data.Camera camera,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(backgroundJobService);
            ArgumentNullException.ThrowIfNull(agent);
            ArgumentNullException.ThrowIfNull(camera);

            var timeZoneOffset = camera.TimezoneOffset ?? agent.TimeZoneOffset;
            var latitude = camera.Latitude ?? agent.Latitude;
            var longitude = camera.Longitude ?? agent.Longitude;
            var sunriseOffset = camera.SunriseOffset ?? agent.SunriseOffset;
            var sunsetOffset = camera.SunsetOffset ?? agent.SunsetOffset;

            var currentTimeInTimezone = DateTimeOffset.UtcNow.AddHours(timeZoneOffset).DateTime;

            var nextSunrise = Celestial.Get_Next_SunRise(
                latitude.Value,
                longitude.Value,
                currentTimeInTimezone,
                timeZoneOffset);

            var nextSunset = Celestial.Get_Next_SunSet(
                latitude.Value,
                longitude.Value,
                currentTimeInTimezone,
                timeZoneOffset);

            var isSunUp = Celestial.CalculateCelestialTimes(
                latitude.Value,
                longitude.Value,
                currentTimeInTimezone,
                timeZoneOffset).IsSunUp;

            var cameraSunriseAt = nextSunrise.AddMinutes(sunriseOffset);
            var cameraSunsetAt = nextSunset.AddMinutes(sunsetOffset);

            if (!string.IsNullOrWhiteSpace(camera.NextDayNightScheduleId))
            {
                backgroundJobService.DeleteJob(camera.NextDayNightScheduleId);
            }

            var scheduleTime = isSunUp ? cameraSunsetAt : cameraSunriseAt;
            var scheduleTimeOffset = new DateTimeOffset(scheduleTime, TimeSpan.FromHours(timeZoneOffset));

            camera.NextDayNightScheduleId = await backgroundJobService.ScheduleProcessSunriseSunsetJobAsync(
                camera.Id,
                isSunUp ? SunriseSunset.Sunset : SunriseSunset.Sunrise,
                true,
                scheduleTimeOffset,
                cancellationToken);
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
