using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraOverlay
{
    public class TestCameraOverlayCommand : IRequest
    {
        public Guid CameraId { get; set; }

        public TestCameraOverlayCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 