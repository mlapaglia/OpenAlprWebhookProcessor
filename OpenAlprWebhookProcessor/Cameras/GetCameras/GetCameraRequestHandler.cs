using Hangfire;
using Hangfire.Storage.Monitoring;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class GetCameraRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly JobStorage _jobStorage;

        public GetCameraRequestHandler(
            ProcessorContext processorContext,
            JobStorage jobStorage)
        {
            _processorContext = processorContext;
            _jobStorage = jobStorage;
        }

        public async Task<List<Camera>> HandleAsync()
        {
            var cameras = new List<Camera>();

            var monitoringApi = _jobStorage.GetMonitoringApi();

            var agent = await _processorContext.Agents.FirstOrDefaultAsync();

            foreach (var camera in await _processorContext.Cameras.ToListAsync())
            {
                JobDetailsDto nextDayNightCommand = null;

                if (!string.IsNullOrWhiteSpace(camera.NextDayNightScheduleId))
                {
                    nextDayNightCommand = monitoringApi.JobDetails(camera.NextDayNightScheduleId);
                }

                cameras.Add(new Camera()
                {
                    Id = camera.Id,
                    CameraPassword = camera.CameraPassword,
                    CameraUsername = camera.CameraUsername,
                    DayNightModeUrl = camera.UpdateDayNightModeUrl,
                    DayNightModeEnabled = camera.UpdateDayNightModeEnabled,
                    DayNightNextScheduledCommand = GetNextScheduledExecutionDate(agent, camera, nextDayNightCommand),
                    Latitude = camera.Latitude,
                    Longitude = camera.Longitude,
                    Manufacturer = camera.Manufacturer,
                    ModelNumber = camera.ModelNumber,
                    OpenAlprCameraId = camera.OpenAlprCameraId,
                    OpenAlprName = camera.OpenAlprName,
                    PlatesSeen = camera.PlatesSeen,
                    SampleImageUrl = await CreateSampleImageUrlAsync(camera.LatestProcessedPlateUuid),
                    SunriseOffset = camera.SunriseOffset,
                    SunsetOffset = camera.SunsetOffset,
                    TimezoneOffset = camera.TimezoneOffset,
                    UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl,
                    UpdateOverlayEnabled = camera.UpdateOverlayEnabled,
                });
            }

            return cameras;
        }

        private async Task<string> CreateSampleImageUrlAsync(string imageUuid)
        {
            var agent = await _processorContext.Agents.FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(imageUuid) || string.IsNullOrEmpty(agent.EndpointUrl))
            {
                return null;
            }

            return Flurl.Url.Combine($"/images/{imageUuid}.jpg");
        }

        private DateTimeOffset? GetNextScheduledExecutionDate(
            Agent agent,
            Data.Camera camera,
            JobDetailsDto dateToEnqueueAt)
        {
            if (dateToEnqueueAt == null || !dateToEnqueueAt.History[0].Data.ContainsKey("EnqueueAt"))
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(dateToEnqueueAt.History[0].Data["EnqueueAt"]))
                .AddHours(camera.TimezoneOffset ?? agent.TimeZoneOffset);
        }
    }
}
