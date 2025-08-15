using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.SetZoomAndFocus
{
    public class SetZoomAndFocusCommand : ICommand
    {
        public Guid CameraId { get; set; }

        public ZoomFocus ZoomAndFocus { get; set; }

        public SetZoomAndFocusCommand(Guid cameraId, ZoomFocus zoomAndFocus)
        {
            CameraId = cameraId;
            ZoomAndFocus = zoomAndFocus;
        }
    }
}