using MediatR;
using OpenAlprWebhookProcessor.Server.Features.Cameras;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameras
{
    public class GetCamerasQuery : IRequest<List<Camera>>
    {
    }
} 