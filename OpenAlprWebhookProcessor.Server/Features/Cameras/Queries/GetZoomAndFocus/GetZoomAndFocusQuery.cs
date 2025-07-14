using MediatR;
using OpenAlprWebhookProcessor.Cameras.ZoomAndFocus;
using System;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus
{
    public class GetZoomAndFocusQuery : IRequest<ZoomFocus>
    {
        public Guid CameraId { get; set; }

        public GetZoomAndFocusQuery(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 