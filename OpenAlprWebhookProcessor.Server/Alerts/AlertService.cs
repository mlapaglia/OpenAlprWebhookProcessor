using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class AlertService : IHostedService
    {
        private readonly BlockingCollection<AlertUpdateRequest> _alertsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly ILogger _logger;

        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        private readonly IEnumerable<IAlertClient> _alertClients;

        public AlertService(
            ILogger<AlertService> logger,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub,
            IEnumerable<IAlertClient> alertClients)
        {
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            _alertsToProcess = new BlockingCollection<AlertUpdateRequest>();
            _processorHub = processorHub;
            _alertClients = alertClients;
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
                _logger.LogInformation("alerting for: {plateNumber}", job.PlateNumber);
                await _processorHub.Clients.All.LicensePlateAlerted(job.PlateNumber);

                foreach (var alertClient in _alertClients)
                {
                    try
                    {
                        await alertClient.SendAlertAsync(job, _cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"failed to send alert to {nameof(alertClient)}");
                    }
                }
            }
        }
    }
}