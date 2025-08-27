using Mediator;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCameraMask
{
    public class UpsertCameraMaskCommandHandler : IQueryHandler<UpsertCameraMaskCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;

        public UpsertCameraMaskCommandHandler(
            IUnitOfWork unitOfWork,
            IWebsocketClientOrganizer websocketClientOrganizer)
        {
            _unitOfWork = unitOfWork;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async ValueTask<bool> Handle(UpsertCameraMaskCommand request, CancellationToken cancellationToken)
        {
            var cameraMask = request.CameraMask;

            var agent = await _unitOfWork.Agents.FirstOrDefaultAsync(x => true, cancellationToken);
            var agentUid = agent?.Uid;

            var camera = await _unitOfWork.Cameras.GetQueryable()
                .Include(x => x.Mask)
                .FirstOrDefaultAsync(x => x.Id == cameraMask.CameraId, cancellationToken);

            if (camera == null)
            {
                return false;
            }

            if (cameraMask.Coordinates.Any())
            {
                if (camera.Mask == null)
                {
                    camera.Mask = new Data.CameraMask()
                    {
                        CameraId = camera.Id,
                        Coordinates = JsonSerializer.Serialize(cameraMask.Coordinates),
                    };
                }
                else
                {
                    camera.Mask.Coordinates = JsonSerializer.Serialize(cameraMask.Coordinates);
                }
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
} 