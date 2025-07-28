using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameras
{
    public class GetCamerasQueryHandler : IQueryHandler<GetCamerasQuery, List<CameraUpdateService.Camera>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobService _backgroundJobService;

        public GetCamerasQueryHandler(IUnitOfWork unitOfWork, IBackgroundJobService backgroundJobService)
        {
            _unitOfWork = unitOfWork;
            _backgroundJobService = backgroundJobService;
        }

        public async ValueTask<List<CameraUpdateService.Camera>> Handle(GetCamerasQuery query, CancellationToken cancellationToken)
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
                    DayNightNextScheduledCommand = GetNextScheduledExecutionDate(camera),
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

        private DateTimeOffset? GetNextScheduledExecutionDate(Data.Camera camera)
        {
            if (string.IsNullOrWhiteSpace(camera.NextDayNightScheduleId))
            {
                return null;
            }

            return _backgroundJobService.GetNextScheduledExecutionTime(camera.NextDayNightScheduleId);
        }
    }
} 