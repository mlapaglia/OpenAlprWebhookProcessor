using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraNightMode
{
    public class TestCameraNightModeCommand : ICommand
    {
        public Guid CameraId { get; set; }

        public TestCameraNightModeCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 