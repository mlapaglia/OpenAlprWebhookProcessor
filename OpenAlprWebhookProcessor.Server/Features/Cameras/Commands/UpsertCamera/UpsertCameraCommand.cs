using Mediator;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCamera
{
    public class UpsertCameraCommand : ICommand
    {
        public CameraUpdateService.Camera Camera { get; set; }

        public UpsertCameraCommand(CameraUpdateService.Camera camera)
        {
            Camera = camera;
        }
    }
} 