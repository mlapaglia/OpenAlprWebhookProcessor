using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus
{
    public class GetZoomAndFocusQueryHandler : IQueryHandler<GetZoomAndFocusQuery, ZoomFocus>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public GetZoomAndFocusQueryHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async ValueTask<ZoomFocus> Handle(GetZoomAndFocusQuery request, CancellationToken cancellationToken = default)
        {
            return await _cameraUpdateService.GetZoomAndFocusAsync(
                request.CameraId,
                cancellationToken);
        }
    }
} 