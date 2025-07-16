using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotWebSocketRelay
{
    public class GetWebSocketSnapshotHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;

        public GetWebSocketSnapshotHandler(
            ProcessorContext processorContext,
            IWebsocketClientOrganizer websocketClientOrganizer)
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
