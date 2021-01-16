using OpenAlprWebhookProcessor.Cameras.Configuration;
using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class Camera
    {
        public Guid Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public CameraManufacturer Manufacturer { get; set; }

        public string OpenAlprName { get; set; }

        public long OpenAlprCameraId { get; set; }

        public string CameraPassword { get; set; }

        public string CameraUsername { get; set; }

        public string UpdateOverlayTextUrl { get; set; }
    }
}
