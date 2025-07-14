using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraNightMode
{
    public class TestCameraNightModeCommand : IRequest
    {
        public Guid CameraId { get; set; }

        public TestCameraNightModeCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 