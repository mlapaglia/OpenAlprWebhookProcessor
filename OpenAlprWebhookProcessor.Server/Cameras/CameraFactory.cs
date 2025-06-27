using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Server.Cameras.Configuration;
using OpenAlprWebhookProcessor.Server.Cameras.Hikvision;
using System;

namespace OpenAlprWebhookProcessor.Server.Cameras
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
