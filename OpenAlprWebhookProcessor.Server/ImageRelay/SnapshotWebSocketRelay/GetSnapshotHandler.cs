using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.ImageRelay.SnapshotWebSocketRelay
{
    public class GetWebSocketSnapshotHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public GetWebSocketSnapshotHandler(
            ProcessorContext processorContext,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _processorContext = processorContext;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<Stream> GetSnapshotAsync(
            string agentId,
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var openAlprCameraId = await _processorContext.Cameras
                .AsNoTracking()
                .Where(x => x.Id == cameraId)
                .Select(x => x.OpenAlprCameraId)
                .FirstOrDefaultAsync(cancellationToken);

            var image = await _websocketClientOrganizer.GetCameraImageAsync(
                agentId,
                openAlprCameraId,
                cancellationToken);

            return image;
        }
    }
}
