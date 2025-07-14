using MediatR;
using OpenAlprWebhookProcessor.Features.Cameras;
using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask
{
    public class GetCameraMaskQuery : IRequest<List<MaskCoordinate>>
    {
        public Guid CameraId { get; set; }

        public GetCameraMaskQuery(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 