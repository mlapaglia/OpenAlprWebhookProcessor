using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Cameras
{
    public class CameraMask
    {
        public Guid AgentId { get; set; }

        public Guid CameraId { get; set; }

        public string ImageMask { get; set; }

        public List<MaskCoordinate> Coordinates { get; set; }
    }
}
