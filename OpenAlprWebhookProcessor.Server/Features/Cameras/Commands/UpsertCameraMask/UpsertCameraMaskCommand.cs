using Mediator;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCameraMask
{
    public class UpsertCameraMaskCommand : IQuery<bool>
    {
        public CameraMask CameraMask { get; set; }

        public UpsertCameraMaskCommand(CameraMask cameraMask)
        {
            CameraMask = cameraMask;
        }
    }
} 