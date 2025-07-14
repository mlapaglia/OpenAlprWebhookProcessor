using MediatR;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameras
{
    public class GetCamerasQuery : IRequest<List<CameraUpdateService.Camera>>
    {
    }
} 