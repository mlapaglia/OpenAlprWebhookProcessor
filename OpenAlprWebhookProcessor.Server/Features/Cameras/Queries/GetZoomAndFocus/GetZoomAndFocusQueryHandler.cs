using MediatR;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus
{
    public class GetZoomAndFocusQueryHandler : IRequestHandler<GetZoomAndFocusQuery, ZoomFocus>
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public GetZoomAndFocusQueryHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task<ZoomFocus> Handle(GetZoomAndFocusQuery request, CancellationToken cancellationToken)
        {
            return await _cameraUpdateService.GetZoomAndFocusAsync(
                request.CameraId,
                cancellationToken);
        }
    }
} 