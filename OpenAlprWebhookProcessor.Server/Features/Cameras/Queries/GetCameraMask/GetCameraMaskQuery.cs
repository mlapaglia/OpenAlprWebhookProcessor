using Mediator;
using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask
{
    public class GetCameraMaskQuery : IQuery<List<MaskCoordinate>>
    {
        public Guid CameraId { get; set; }

        public GetCameraMaskQuery(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 