using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.UpdatedCameras
{
    public class UpsertCameraHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertCameraHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task UpsertCameraAsync(Camera camera)
        {
            var existingCamera = await _processorContext.Cameras
                .FirstOrDefaultAsync(x => x.OpenAlprCameraId == camera.OpenAlprCameraId);

            if (existingCamera == null)
            {
                existingCamera = new Data.Camera()
                {
                    CameraPassword = camera.CameraPassword,
                    CameraUsername = camera.CameraUsername,
                    Latitude = camera.Latitude,
                    Longitude = camera.Longitude,
                    Manufacturer = camera.Manufacturer,
                    OpenAlprCameraId = camera.OpenAlprCameraId,
                    OpenAlprName = camera.OpenAlprName,
                    UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl.ToString(),
                };

                _processorContext.Add(existingCamera);
            }
            else
            {
                existingCamera.CameraPassword = camera.CameraPassword;
                existingCamera.CameraUsername = camera.CameraUsername;
                existingCamera.Latitude = camera.Latitude;
                existingCamera.Longitude = camera.Longitude;
                existingCamera.Manufacturer = camera.Manufacturer;
                existingCamera.OpenAlprCameraId = camera.OpenAlprCameraId;
                existingCamera.OpenAlprName = camera.OpenAlprName;
                existingCamera.UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl.ToString();
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
