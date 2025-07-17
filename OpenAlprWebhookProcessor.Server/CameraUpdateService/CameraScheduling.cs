using CoordinateSharp;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraScheduling : ICameraScheduling
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceProvider _serviceProvider;

        public CameraScheduling(IBackgroundJobClient backgroundJobClient, IServiceProvider serviceProvider)
        {
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void ExecuteSingleDayNightTask(
            SunriseSunset sunriseSunset,
            Guid cameraId,
            IBackgroundJobService backgroundJobService)
        {
            if (backgroundJobService == null)
                throw new ArgumentNullException(nameof(backgroundJobService));

            backgroundJobService.EnqueueProcessSunriseSunsetJob(
                cameraId,
                sunriseSunset,
                false);
        }

        public async Task ScheduleDayNightTasksAsync(
            IBackgroundJobService backgroundJobService)
        {
            if (backgroundJobService == null)
                throw new ArgumentNullException(nameof(backgroundJobService));

            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var camerasToUpdate = await unitOfWork.Cameras.FindAsync(x => x.UpdateDayNightModeEnabled);

                var agent = await unitOfWork.Agents.GetFirstAgentAsync();

                foreach (var camera in camerasToUpdate)
                {
                    ScheduleDayNightTask(
                        backgroundJobService,
                        agent,
                        camera);
                }

                await unitOfWork.SaveChangesAsync();
            }
        }

        public void ScheduleDayNightTask(
            IBackgroundJobService backgroundJobService,
            Agent agent,
            Data.Camera camera)
        {
            if (backgroundJobService == null)
                throw new ArgumentNullException(nameof(backgroundJobService));
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

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

        public bool IsSunUp(double latitude, double longitude)
        {
            var cameraCoordinate = new Coordinate(
                latitude,
                longitude,
                DateTime.UtcNow);

            return cameraCoordinate.CelestialInfo.IsSunUp;
        }
    }
}
