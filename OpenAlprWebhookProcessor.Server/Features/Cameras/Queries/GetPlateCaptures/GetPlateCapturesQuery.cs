using MediatR;
using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetPlateCaptures
{
    public class GetPlateCapturesQuery : IRequest<List<string>>
    {
        public Guid CameraId { get; set; }

        public GetPlateCapturesQuery(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 