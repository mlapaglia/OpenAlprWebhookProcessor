using System;

namespace OpenAlprWebhookProcessor.Cameras.Configuration
{
    public class HikvisionCameraConfiguration
    {
        public Uri UpdateTextUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int OpenAlprCameraId { get; set; }
    }
}
