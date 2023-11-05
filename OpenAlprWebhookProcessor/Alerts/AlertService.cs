using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class AlertService : IHostedService
    {
        private readonly BlockingCollection<AlertUpdateRequest> _alertsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger _logger;

        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        private readonly IAlertClient _alertClient;

        public AlertService(
            IServiceProvider serviceProvider,
            ILogger<AlertService> logger,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub,
            IAlertClient alertClient)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
            _alertsToProcess = new BlockingCollection<AlertUpdateRequest>();
            _processorHub = processorHub;
            _alertClient = alertClient;
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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                    var plateGroups = processorContext.PlateGroups.AsQueryable();

                    if (job.IsStrictMatch)
                    {
                        plateGroups = plateGroups.Where(x => x.Id == job.LicensePlateId || x.PossibleNumbers.Any(x => x.Number == job.LicensePlateId.ToString()));
                    }
                    else
                    {
                        plateGroups = plateGroups.Where(x => x.Id == job.LicensePlateId);
                    }

                    var result = await plateGroups
                        .Include(x => x.PlateImage)
                        .FirstOrDefaultAsync(_cancellationTokenSource.Token);

                    if (result != null)
                    {
                        _logger.LogInformation("alerting for: {alertId}", result.Id);
                        await _processorHub.Clients.All.LicensePlateAlerted(result.Id.ToString());

                        try
                        {
                            await _alertClient.SendAlertAsync(new Alert()
                            {
                                Description = job.Description,
                                Id = result.Id,
                                PlateNumber = result.BestNumber,
                            },
                            result.PlateImage.Jpeg,
                            _cancellationTokenSource.Token);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"failed to send alert to {nameof(_alertClient)}");
                        }
                    }
                }
            }
        }
    }
}