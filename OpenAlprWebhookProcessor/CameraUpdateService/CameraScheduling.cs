using CoordinateSharp;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public static class CameraScheduling
    {
        public static async Task ScheduleDayNightTaskAsync(
            CameraUpdateService cameraUpdateService,
            IServiceProvider serviceProvider,
            IBackgroundJobClient backgroundJobClient)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var camerasToUpdate = await processorContext.Cameras.ToListAsync();

                var agent = await processorContext.Agents.FirstOrDefaultAsync();

                foreach (var camera in camerasToUpdate)
                {
                    var timeZoneOffset = camera.TimezoneOffset ?? agent.TimeZoneOffset;
                    var latitude = camera.Latitude ?? agent.Latitude;
                    var longitude = camera.Longitude ?? agent.Longitude;
                    var sunriseOffset = camera.SunriseOffset ?? agent.SunriseOffset;
                    var sunsetOffset = camera.SunsetOffset ?? agent.SunsetOffset;

                    var nextSunrise = Celestial.Get_Next_SunRise(
                        latitude,
                        longitude,
                        DateTime.UtcNow,
                        timeZoneOffset);

                    var nextSunset = Celestial.Get_Next_SunSet(
                        latitude,
                        longitude,
                        DateTime.UtcNow,
                        timeZoneOffset);

                    var isSunUp = Celestial.CalculateCelestialTimes(
                        latitude,
                        longitude,
                        DateTime.UtcNow,
                        timeZoneOffset).IsSunUp;

                    var cameraSunriseAt = nextSunrise.AddHours(sunriseOffset);
                    var cameraSunsetAt = nextSunset.AddHours(sunsetOffset);

                    if (!string.IsNullOrWhiteSpace(camera.NextDayNightScheduleId))
                    {
                        backgroundJobClient.Delete(camera.NextDayNightScheduleId);
                    }

                    camera.NextDayNightScheduleId = backgroundJobClient.Schedule(
                        () => cameraUpdateService.ProcessSunriseSunsetJobAsync(
                            camera.Id,
                            isSunUp ? SunriseSunset.Sunset : SunriseSunset.Sunrise),
                            isSunUp ? cameraSunsetAt : cameraSunriseAt);
                }

                await processorContext.SaveChangesAsync();
            }
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
