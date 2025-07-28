using Mediator;
using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetPlateCaptures
{
    public class GetPlateCapturesQuery : IQuery<List<string>>
    {
        public Guid CameraId { get; set; }

        public GetPlateCapturesQuery(Guid cameraId)
        {
            CameraId = cameraId;
        }
    }
} 