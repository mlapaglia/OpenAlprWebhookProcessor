using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class GetCameraRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetCameraRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<Camera>> HandleAsync()
        {
            var cameras = new List<Camera>();

            foreach(var camera in await _processorContext.Cameras.ToListAsync())
            {
                cameras.Add(new Camera()
                {
                    Id = camera.Id,
                    CameraPassword = camera.CameraPassword,
                    CameraUsername = camera.CameraUsername,
                    Latitude = camera.Latitude,
                    Longitude = camera.Longitude,
                    Manufacturer = camera.Manufacturer,
                    ModelNumber = camera.ModelNumber,
                    OpenAlprCameraId = camera.OpenAlprCameraId,
                    OpenAlprName = camera.OpenAlprName,
                    PlatesSeen = camera.PlatesSeen,
                    UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl,
                    UpdateOverlayEnabled = camera.UpdateOverlayEnabled,
                    DayNightModeUrl = camera.UpdateDayNightModeUrl,
                    DayNightModeEnabled = camera.UpdateDayNightModeEnabled,
                    SampleImageUrl = await CreateSampleImageUrlAsync(camera.LatestProcessedPlateUuid),
                    SunriseOffset = camera.SunriseOffset,
                    SunsetOffset = camera.SunsetOffset,
                    TimezoneOffset = camera.TimezoneOffset,
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
    }
}
