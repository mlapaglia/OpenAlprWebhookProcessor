using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCameraMask
{
    public class UpsertCameraMaskCommandHandler : IRequestHandler<UpsertCameraMaskCommand, bool>
    {
        private readonly ProcessorContext _processorContext;
        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public UpsertCameraMaskCommandHandler(
            ProcessorContext processorContext,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _processorContext = processorContext;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<bool> Handle(UpsertCameraMaskCommand request, CancellationToken cancellationToken)
        {
            var cameraMask = request.CameraMask;

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