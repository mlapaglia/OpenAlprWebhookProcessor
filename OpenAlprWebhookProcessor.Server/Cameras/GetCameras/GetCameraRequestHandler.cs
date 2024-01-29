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
                    IpAddress = camera.IpAddress,
                    Latitude = camera.Latitude ?? agent?.Latitude ?? null,
                    Longitude = camera.Longitude ?? agent?.Longitude ?? null,
                    Manufacturer = camera.Manufacturer,
                    ModelNumber = camera.ModelNumber,
                    DayFocus = camera.DayFocus,
                    DayZoom = camera.DayZoom,
                    NightFocus = camera.NightFocus,
                    NightZoom = camera.NightZoom,
                    OpenAlprCameraId = camera.OpenAlprCameraId,
                    OpenAlprName = camera.OpenAlprName,
                    OpenAlprEnabled = camera.OpenAlprEnabled,
                    PlatesSeen = camera.PlatesSeen,
                    SampleImageUrl = await CreateSampleImageUrlAsync(camera),
                    SunriseOffset = camera.SunriseOffset,
                    SunsetOffset = camera.SunsetOffset,
                    TimezoneOffset = camera.TimezoneOffset,
                    UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl,
                    UpdateOverlayEnabled = camera.UpdateOverlayEnabled,
                });
            }

            return cameras;
        }

        private async Task<string> CreateSampleImageUrlAsync(
            Data.Camera camera)
        {
            if (camera.UpdateOverlayEnabled)
            {
                var agent = await _processorContext.Agents.FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(camera.LatestProcessedPlateUuid) || string.IsNullOrEmpty(agent.EndpointUrl))
                {
                    return null;
                }

                return Flurl.Url.Combine($"/api/images/{camera.LatestProcessedPlateUuid}");
            }
            else
            {
                return Flurl.Url.Combine($"/api/images/{camera.Id}/snapshot");
            }
        }

        private static DateTimeOffset? GetNextScheduledExecutionDate(
            Agent agent,
            Data.Camera camera,
            JobDetailsDto dateToEnqueueAt)
        {
            if (dateToEnqueueAt == null || !dateToEnqueueAt.History[0].Data.ContainsKey("EnqueueAt"))
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeMilliseconds(
                Convert.ToInt64(dateToEnqueueAt.History[0].Data["EnqueueAt"]))
                .ToOffset(TimeSpan.FromHours(camera.TimezoneOffset ?? agent.TimeZoneOffset));
        }
    }
}
