using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode
{
    public class TestCameraDayModeCommand : IRequest
    {
        public Guid CameraId { get; set; }

        public TestCameraDayModeCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 