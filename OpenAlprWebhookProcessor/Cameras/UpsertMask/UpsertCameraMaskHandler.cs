using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class UpsertCameraMaskHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public UpsertCameraMaskHandler(
            ProcessorContext processorContext,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _processorContext = processorContext;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<bool> UpsertCameraMaskAsync(
            UpsertMasks.CameraMask cameraMask,
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
                camera.Mask = new CameraMask()
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
