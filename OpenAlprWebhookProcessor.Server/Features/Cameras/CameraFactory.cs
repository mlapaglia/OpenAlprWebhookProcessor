using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.CameraUpdateService.Hikvision;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras
{
    public static class CameraFactory
    {
        public static ICamera Create(
            CameraManufacturer cameraManufacturer,
            Data.Camera camera)
        {
            return cameraManufacturer switch
            {
                CameraManufacturer.Dahua => new DahuaCamera(camera),
                CameraManufacturer.Hikvision => new HikvisionCamera(camera),
                _ => throw new ArgumentException("unknown camera manufacturer"),
            };
        }
    }
}
