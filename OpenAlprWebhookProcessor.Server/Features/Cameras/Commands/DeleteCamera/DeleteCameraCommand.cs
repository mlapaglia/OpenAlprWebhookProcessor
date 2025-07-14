using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera
{
    public class DeleteCameraCommand : IRequest
    {
        public Guid CameraId { get; set; }

        public DeleteCameraCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 