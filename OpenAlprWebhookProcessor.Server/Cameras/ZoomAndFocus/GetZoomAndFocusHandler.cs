using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras.ZoomAndFocus
{
    public class GetZoomAndFocusHandler
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public GetZoomAndFocusHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task<ZoomFocus> HandleAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            return await _cameraUpdateService.GetZoomAndFocusAsync(
                cameraId,
                cancellationToken);
        }
    }
}
