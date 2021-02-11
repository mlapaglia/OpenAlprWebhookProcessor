using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class UpsertCameraHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public UpsertCameraHandler(
            ProcessorContext processorContext,
            CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _processorContext = processorContext;
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task UpsertCameraAsync(Camera camera)
        {
            var existingCamera = await _processorContext.Cameras
                .FirstOrDefaultAsync(x => x.Id == camera.Id);

            if (existingCamera == null)
            {
                existingCamera = new Data.Camera()
                {
                    Id = camera.Id,
                    CameraPassword = camera.CameraPassword,
                    CameraUsername = camera.CameraUsername,
                    IpAddress = camera.IpAddress,
                    Latitude = camera.Latitude,
                    Longitude = camera.Longitude,
                    Manufacturer = camera.Manufacturer,
                    OpenAlprCameraId = camera.OpenAlprCameraId,
                    OpenAlprName = camera.OpenAlprName,
                    OpenAlprEnabled = camera.OpenAlprEnabled,
                    UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl,
                    UpdateOverlayEnabled = camera.UpdateOverlayEnabled,
                    UpdateDayNightModeUrl = camera.DayNightModeUrl,
                    UpdateDayNightModeEnabled = camera.DayNightModeEnabled,
                    SunriseOffset = camera.SunriseOffset,
                    SunsetOffset = camera.SunsetOffset,
                    TimezoneOffset = camera.TimezoneOffset,
                };

                _processorContext.Add(existingCamera);
            }
            else
            {
                existingCamera.CameraPassword = camera.CameraPassword;
                existingCamera.CameraUsername = camera.CameraUsername;
                existingCamera.IpAddress = camera.IpAddress;
                existingCamera.Latitude = camera.Latitude;
                existingCamera.Longitude = camera.Longitude;
                existingCamera.Manufacturer = camera.Manufacturer;
                existingCamera.ModelNumber = camera.ModelNumber;
                existingCamera.OpenAlprCameraId = camera.OpenAlprCameraId;
                existingCamera.OpenAlprName = camera.OpenAlprName;
                existingCamera.OpenAlprEnabled = camera.OpenAlprEnabled;
                existingCamera.UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl;
                existingCamera.UpdateOverlayEnabled = camera.UpdateOverlayEnabled;
                existingCamera.UpdateDayNightModeUrl = camera.DayNightModeUrl;
                existingCamera.UpdateDayNightModeEnabled = camera.DayNightModeEnabled;
                existingCamera.SunriseOffset = camera.SunriseOffset;
                existingCamera.SunsetOffset = camera.SunsetOffset;
                existingCamera.TimezoneOffset = camera.TimezoneOffset;
            }

            await _processorContext.SaveChangesAsync();

            if (existingCamera.UpdateDayNightModeEnabled)
            {
                await _cameraUpdateService.ScheduleDayNightTaskAsync();
            }
            else
            {
                await _cameraUpdateService.DeleteSunriseSunsetAsync(existingCamera.Id);
            }
        }
    }
}
