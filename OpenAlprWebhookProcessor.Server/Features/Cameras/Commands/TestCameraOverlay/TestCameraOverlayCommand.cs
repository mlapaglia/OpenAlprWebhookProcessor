using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraOverlay
{
    public class TestCameraOverlayCommand : ICommand
    {
        public Guid CameraId { get; set; }

        public TestCameraOverlayCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 