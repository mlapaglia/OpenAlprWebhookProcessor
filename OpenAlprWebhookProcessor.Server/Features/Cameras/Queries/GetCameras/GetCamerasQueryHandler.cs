using MediatR;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameras
{
    public class GetCamerasQueryHandler : IRequestHandler<GetCamerasQuery, List<CameraUpdateService.Camera>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCamerasQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CameraUpdateService.Camera>> Handle(GetCamerasQuery request, CancellationToken cancellationToken)
        {
            var cameras = new List<CameraUpdateService.Camera>();

            var agent = await _unitOfWork.Agents.FirstOrDefaultAsync(x => true, cancellationToken);

            foreach (var camera in await _unitOfWork.Cameras.GetAllAsync(cancellationToken))
            {
                cameras.Add(new CameraUpdateService.Camera()
                {
                    Id = camera.Id,
                    CameraPassword = camera.CameraPassword,
                    CameraUsername = camera.CameraUsername,
                    DayNightModeUrl = camera.UpdateDayNightModeUrl,
                    DayNightModeEnabled = camera.UpdateDayNightModeEnabled,
                    DayNightNextScheduledCommand = GetNextScheduledExecutionDate(agent, camera),
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
                var agent = await _unitOfWork.Agents.FirstOrDefaultAsync(x => true, default);

                if (string.IsNullOrEmpty(camera.LatestProcessedPlateUuid) || string.IsNullOrEmpty(agent?.EndpointUrl))
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
            Data.Camera camera)
        {
            // Since we're using a timer-based system now, we don't track individual job schedules
            // This could be enhanced to track next execution times if needed
            // For now, return null to indicate no specific scheduled time is available
            return null;
        }
    }
} 