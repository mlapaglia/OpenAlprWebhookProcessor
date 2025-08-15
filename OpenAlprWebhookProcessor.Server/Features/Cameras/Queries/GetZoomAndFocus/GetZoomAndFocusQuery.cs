using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus
{
    public class GetZoomAndFocusQuery : IQuery<ZoomFocus>
    {
        public Guid CameraId { get; set; }

        public GetZoomAndFocusQuery(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 