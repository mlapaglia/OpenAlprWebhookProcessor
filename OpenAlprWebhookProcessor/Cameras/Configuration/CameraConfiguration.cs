using System;

namespace OpenAlprWebhookProcessor.Cameras.Configuration
{
    public class CameraConfiguration
    {
        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public CameraManufacturer Manufacturer { get; set; }

        public string Name { get; set; }

        public int OpenAlprCameraId { get; set; }

        public string Password { get; set; }

        public Uri UpdateOverlayTextUrl { get; set; }

        public string Username { get; set; }
    }
}
