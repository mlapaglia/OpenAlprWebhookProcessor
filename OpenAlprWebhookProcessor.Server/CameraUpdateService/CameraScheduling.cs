using CoordinateSharp;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public static class CameraScheduling
    {
        public static void ExecuteSingleDayNightTask(
            SunriseSunset sunriseSunset,
            Guid cameraId,
            CameraUpdateService cameraUpdateService,
            IBackgroundJobClient backgroundJobClient)
        {
            backgroundJobClient.Enqueue(
                () => cameraUpdateService.ProcessSunriseSunsetJobAsync(
                    cameraId,
                    sunriseSunset,
                    false));
        }

        public static async Task ScheduleDayNightTasksAsync(
            CameraUpdateService cameraUpdateService,
            IServiceProvider serviceProvider,
            IBackgroundJobClient backgroundJobClient)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var camerasToUpdate = await unitOfWork.Cameras.FindAsync(x => x.UpdateDayNightModeEnabled);

                var agent = await unitOfWork.Agents.GetFirstAgentAsync();

                foreach (var camera in camerasToUpdate)
                {
                    ScheduleDayNightTask(
                        cameraUpdateService,
                        backgroundJobClient,
                        agent,
                        camera);
                }

                await unitOfWork.SaveChangesAsync();
            }
        }

        public static void ScheduleDayNightTask(
            CameraUpdateService cameraUpdateService,
            IBackgroundJobClient backgroundJobClient,
            Agent agent,
            Data.Camera camera)
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
