using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public class AlertHostedService : BackgroundService
    {
        private readonly IAlertService _alertService;

        private readonly ILogger<AlertHostedService> _logger;

        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        private readonly IEnumerable<IAlertClient> _alertClients;

        public AlertHostedService(
            IAlertService alertService,
            ILogger<AlertHostedService> logger,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub,
            IEnumerable<IAlertClient> alertClients)
        {
            _alertService = alertService;
            _logger = logger;
            _processorHub = processorHub;
            _alertClients = alertClients;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("AlertHostedService starting...");

            try
            {
                await ProcessAlertsAsync(stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Alert processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in AlertHostedService");
                throw;
            }

            _logger.LogDebug("AlertHostedService stopped.");
        }

        private async Task ProcessAlertsAsync(CancellationToken cancellationToken)
        {
            foreach (var job in _alertService.GetConsumingAlerts(cancellationToken))
            {
                try
                {
                    await ProcessSingleAlert(job, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing alert for plate: {PlateNumber}", job.PlateNumber);
                }
            }
        }

        private async Task ProcessSingleAlert(AlertUpdateRequest job, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing alert for: {PlateNumber}. Pending alerts: {Count}",
                job.PlateNumber,
                _alertService.GetPendingAlertsCount());

            try
            {
                await _processorHub.Clients.All.LicensePlateAlerted(job.PlateNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SignalR notification for plate: {PlateNumber}", job.PlateNumber);
            }

            var sendTasks = new List<Task>();

            foreach (var alertClient in _alertClients)
            {
                var clientTask = SendAlertToClientAsync(alertClient, job, cancellationToken);
                sendTasks.Add(clientTask);
            }

            await Task.WhenAll(sendTasks);
        }

        private async Task SendAlertToClientAsync(IAlertClient alertClient, AlertUpdateRequest job, CancellationToken cancellationToken)
        {
            try
            {
                await alertClient.SendAlertAsync(job, cancellationToken);
                _logger.LogDebug("Successfully sent alert to {ClientType} for plate: {PlateNumber}",
                    alertClient.GetType().Name,
                    job.PlateNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert to {ClientType} for plate: {PlateNumber}",
                    alertClient.GetType().Name,
                    job.PlateNumber);
            }
        }
    }
}