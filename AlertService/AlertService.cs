using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Concurrent;
using System.Linq;
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

        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        public AlertService(
            IServiceScopeFactory scopeFactory,
            ILogger<AlertService> logger,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _cancellationTokenSource = new CancellationTokenSource();
            _alertsToProcess = new BlockingCollection<AlertUpdateRequest>();
            _processorHub = processorHub;
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
                    var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                    var plate = await processorContext.PlateGroups.Where(x => x.Id == job.LicensePlateId).FirstOrDefaultAsync();

                    await _processorHub.Clients.All.LicensePlateAlerted(plate.Id.ToString());
                }
            }
        }
    }
}