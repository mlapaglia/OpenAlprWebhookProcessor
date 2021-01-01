using System;

namespace OpenAlprWebhookProcessor.Cameras.Configuration
{
    public class Camera
    {
        public CameraManufacturer Manufacturer { get; set; }

        public Uri UpdateOverlayTextUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int OpenAlprCameraId { get; set; }
    }
}
