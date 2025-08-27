using Mediator;
using System;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.WebsocketSnapshotRelay
{
    public class GetWebsocketSnapshotQuery : IQuery<Stream>
    {
        public string AgentId { get; set; }
        
        public long CameraId { get; set; }

        public GetWebsocketSnapshotQuery(string agentId, long cameraId)
        {
            AgentId = agentId;
            CameraId = cameraId;
        }
    }
}
