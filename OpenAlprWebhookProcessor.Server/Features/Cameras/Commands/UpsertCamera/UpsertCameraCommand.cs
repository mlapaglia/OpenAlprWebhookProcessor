using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCamera
{
    public class UpsertCameraCommand : ICommand
    {
        public Camera Camera { get; set; }

        public UpsertCameraCommand(Camera camera)
        {
            Camera = camera;
        }
    }
} 