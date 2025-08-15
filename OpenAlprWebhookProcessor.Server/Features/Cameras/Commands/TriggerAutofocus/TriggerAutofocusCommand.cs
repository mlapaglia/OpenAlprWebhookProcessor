using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus
{
    public class TriggerAutofocusCommand : IQuery<bool>
    {
        public Guid CameraId { get; set; }

        public TriggerAutofocusCommand(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 