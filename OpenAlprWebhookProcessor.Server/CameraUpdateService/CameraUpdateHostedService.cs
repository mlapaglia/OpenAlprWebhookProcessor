using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateHostedService : IHostedService
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        private readonly ILogger<CameraUpdateHostedService> _logger;

        public CameraUpdateHostedService(
            ICameraUpdateService cameraUpdateService,
            ILogger<CameraUpdateHostedService> logger)
        {
            _cameraUpdateService = cameraUpdateService ?? throw new ArgumentNullException(nameof(cameraUpdateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("CameraUpdateHostedService starting...");

            try
            {
                await _cameraUpdateService.ScheduleDayNightTaskAsync(cancellationToken);
                _logger.LogDebug("Initial day/night tasks scheduled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling initial day/night tasks");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("CameraUpdateHostedService stopping...");

            try
            {
                await _cameraUpdateService.ForceClearOverlaysAsync(cancellationToken);
                _logger.LogDebug("All camera overlays cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing camera overlays during shutdown");
            }

            _logger.LogDebug("CameraUpdateHostedService stopped.");
        }
    }
}