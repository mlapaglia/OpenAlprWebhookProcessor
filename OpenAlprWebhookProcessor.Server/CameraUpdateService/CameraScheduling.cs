using CoordinateSharp;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Server.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.CameraUpdateService
{
    public static class CameraScheduling
    {
        public static void ExecuteSingleDayNightTask(
            SunriseSunset sunriseSunset,
            Guid cameraId,
            ICameraUpdateService cameraUpdateService,
            IBackgroundJobClient backgroundJobClient)
        {
            backgroundJobClient.Enqueue(
                () => cameraUpdateService.ProcessSunriseSunsetJobAsync(
                    cameraId,
                    sunriseSunset,
                    false));
        }

        public static async Task ScheduleDayNightTasksAsync(
            ICameraUpdateService cameraUpdateService,
            IServiceProvider serviceProvider,
            IBackgroundJobClient backgroundJobClient,
            CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var camerasToUpdate = await processorContext.Cameras.ToListAsync(cancellationToken);

                var agent = await processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

                foreach (var camera in camerasToUpdate.Where(x => x.UpdateDayNightModeEnabled))
                {
                    ScheduleDayNightTask(
                        cameraUpdateService,
                        backgroundJobClient,
                        agent,
                        camera);
                }

                await processorContext.SaveChangesAsync(cancellationToken);
            }
        }

        public static void ScheduleDayNightTask(
            ICameraUpdateService cameraUpdateService,
            IBackgroundJobClient backgroundJobClient,
            Agent agent,
            Camera camera)
        {
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
                backgroundJobClient.Delete(camera.NextDayNightScheduleId);
            }

            camera.NextDayNightScheduleId = backgroundJobClient.Schedule(
                () => cameraUpdateService.ProcessSunriseSunsetJobAsync(
                    camera.Id,
                    isSunUp ? SunriseSunset.Sunset : SunriseSunset.Sunrise,
                    true),
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
