using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;

namespace OpenAlprWebhookProcessor.Features.Cameras
{
    public interface ICameraFactory
    {
        ICamera Create(CameraManufacturer cameraManufacturer, Data.Camera camera);
    }
} 