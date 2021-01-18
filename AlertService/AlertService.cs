using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.AlertService
{
    public class AlertService : IHostedService
    {
        private readonly BlockingCollection<AlertUpdateRequest> _alertsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger _logger;

        public AlertService(
            IServiceScopeFactory scopeFactory,
            ILogger logger)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _cancellationTokenSource = new CancellationTokenSource();
            _alertsToProcess = new BlockingCollection<AlertUpdateRequest>();
        }

        public void AddJob(AlertUpdateRequest request)
        {
            _logger.LogInformation("adding job for alert: ");
            _alertsToProcess.Add(request);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
                await ProcessAlertsAsync(),
                cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        private async Task ProcessAlertsAsync()
        {
            foreach (var job in _alertsToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                }
            }
        }
    }
}