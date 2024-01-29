using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras.ZoomAndFocus
{
    public class SetZoomAndFocusHandler
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public SetZoomAndFocusHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task HandleAsync(
            Guid cameraId,
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken)
        {
            await _cameraUpdateService.SetZoomAndFocusAsync(
                cameraId,
                zoomAndFocus,
                cancellationToken);
        }
    }
}
