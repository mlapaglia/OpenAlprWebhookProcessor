using MediatR;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCamera
{
    public class UpsertCameraCommand : IRequest
    {
        public CameraUpdateService.Camera Camera { get; set; }

        public UpsertCameraCommand(CameraUpdateService.Camera camera)
        {
            Camera = camera;
        }
    }
} 