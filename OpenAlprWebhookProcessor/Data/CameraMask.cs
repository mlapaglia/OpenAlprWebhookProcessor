using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class CameraMask
    {
        public Guid Id { get; set; }

        public Guid CameraId { get; set; }

        public Camera Camera { get; set; }

        public string Coordinates { get; set; }
    }
}
