using MediatR;
using OpenAlprWebhookProcessor.Features.Cameras;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCamera
{
    public class UpsertCameraCommand : IRequest
    {
        public Camera Camera { get; set; }

        public UpsertCameraCommand(Camera camera)
        {
            Camera = camera;
        }
    }
} 