using Flurl.Http.Configuration;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.CameraUpdateService.Hikvision;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras
{
    public class CameraFactory : ICameraFactory
    {
        private readonly IFlurlClientCache _flurlClientCache;

        public CameraFactory(IFlurlClientCache flurlClientCache)
        {
            _flurlClientCache = flurlClientCache;
        }

        public ICamera Create(
            CameraManufacturer cameraManufacturer,
            Data.Camera camera)
        {
            return cameraManufacturer switch
            {
                CameraManufacturer.Dahua => new DahuaCamera(camera, _flurlClientCache),
                CameraManufacturer.Hikvision => new HikvisionCamera(camera, _flurlClientCache),
                _ => throw new ArgumentException("unknown camera manufacturer"),
            };
        }
    }
}
