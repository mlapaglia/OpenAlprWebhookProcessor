using System.Threading.Tasks;
using System.Threading;
using System;

namespace OpenAlprWebhookProcessor.Cameras.ZoomAndFocus
{
    public class TriggerAutofocusHandler
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public TriggerAutofocusHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task<bool> HandleAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            return await _cameraUpdateService.TriggerAutofocusAsync(
                cameraId,
                cancellationToken);
        }
    }
}
