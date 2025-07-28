using Mediator;
using System;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotRelay
{
    public class GetSnapshotQuery : IQuery<Stream>
    {
        public Guid CameraId { get; set; }

        public GetSnapshotQuery(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 