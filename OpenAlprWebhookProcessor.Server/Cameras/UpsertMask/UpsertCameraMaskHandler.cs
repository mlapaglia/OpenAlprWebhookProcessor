using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.Cameras.UpsertMask
{
    public class UpsertCameraMaskHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public UpsertCameraMaskHandler(
            ProcessorContext processorContext,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _processorContext = processorContext;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<bool> UpsertCameraMaskAsync(
            CameraMask cameraMask,
            CancellationToken cancellationToken)
        {
            var agentUid = await _processorContext.Agents
                .Select(x => x.Uid)
                .FirstOrDefaultAsync(cancellationToken);

            var camera = await _processorContext.Cameras
                .Include(x => x.Mask)
                .FirstOrDefaultAsync(x => x.Id == cameraMask.CameraId, cancellationToken);

            if (cameraMask.Coordinates.Any())
            {
                camera.Mask = new Data.CameraMask()
                {
                    Coordinates = JsonSerializer.Serialize(cameraMask.Coordinates),
                };
            }
            else
            {
                camera.Mask = null;
            }

            var result = await _websocketClientOrganizer.UpsertCameraMaskAsync(
                agentUid,
                cameraMask.ImageMask,
                camera.OpenAlprName,
                cancellationToken);

            await _processorContext.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
