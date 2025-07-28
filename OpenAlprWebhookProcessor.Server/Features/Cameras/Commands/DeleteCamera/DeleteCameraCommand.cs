using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera
{
    public class DeleteCameraCommand : ICommand
    {
        public Guid CameraId { get; set; }

        public DeleteCameraCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 