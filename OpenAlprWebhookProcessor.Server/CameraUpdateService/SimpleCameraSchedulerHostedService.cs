using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class SimpleCameraSchedulerHostedService : BackgroundService
    {
        private readonly ISimpleCameraScheduler _cameraScheduler;
        private readonly ILogger<SimpleCameraSchedulerHostedService> _logger;

        public SimpleCameraSchedulerHostedService(
            ISimpleCameraScheduler cameraScheduler,
            ILogger<SimpleCameraSchedulerHostedService> logger)
        {
            _cameraScheduler = cameraScheduler ?? throw new ArgumentNullException(nameof(cameraScheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("SimpleCameraScheduler hosted service starting");
                await _cameraScheduler.StartAsync(stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "SimpleCameraScheduler hosted service stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SimpleCameraScheduler hosted service encountered an error");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SimpleCameraScheduler hosted service stop requested");
            await _cameraScheduler.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
