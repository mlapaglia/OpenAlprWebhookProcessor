using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus
{
    public class GetZoomAndFocusQueryHandler : IQueryHandler<GetZoomAndFocusQuery, ZoomFocus>
    {
        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public GetZoomAndFocusQueryHandler(ISimpleCameraScheduler simpleCameraScheduler)
        {
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public async ValueTask<ZoomFocus> Handle(GetZoomAndFocusQuery request, CancellationToken cancellationToken = default)
        {
            return await _simpleCameraScheduler.GetZoomAndFocusAsync(
                request.CameraId,
                cancellationToken);
        }
    }
} 