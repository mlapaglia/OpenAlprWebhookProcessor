using MediatR;
using OpenAlprWebhookProcessor.Cameras.UpsertMasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCameraMask
{
    public class UpsertCameraMaskCommand : IRequest<bool>
    {
        public CameraMask CameraMask { get; set; }

        public UpsertCameraMaskCommand(CameraMask cameraMask)
        {
            CameraMask = cameraMask;
        }
    }
} 