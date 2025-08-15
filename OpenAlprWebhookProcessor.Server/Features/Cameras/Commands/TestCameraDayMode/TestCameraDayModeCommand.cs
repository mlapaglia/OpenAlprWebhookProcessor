using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode
{
    public class TestCameraDayModeCommand : ICommand
    {
        public Guid CameraId { get; set; }

        public TestCameraDayModeCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 