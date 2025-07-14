using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus
{
    public class TriggerAutofocusCommand : IRequest<bool>
    {
        public Guid CameraId { get; set; }

        public TriggerAutofocusCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 