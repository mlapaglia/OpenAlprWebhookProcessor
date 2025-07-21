using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class WebsocketClientOrganizerHostedService : BackgroundService
    {
        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;

        private readonly ILogger<WebsocketClientOrganizerHostedService> _logger;

        public WebsocketClientOrganizerHostedService(
            IWebsocketClientOrganizer websocketClientOrganizer,
            ILogger<WebsocketClientOrganizerHostedService> logger)
        {
            _websocketClientOrganizer = websocketClientOrganizer ?? throw new ArgumentNullException(nameof(websocketClientOrganizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("WebsocketClientOrganizerHostedService starting...");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "WebsocketClientOrganizerHostedService cancellation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in WebsocketClientOrganizerHostedService");
                throw;
            }
            finally
            {
                _logger.LogInformation("Disconnecting all websocket clients...");

                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    await _websocketClientOrganizer.DisconnectAllClientsAsync(cts.Token);
                    _logger.LogInformation("All websocket clients disconnected");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disconnecting websocket clients during shutdown");
                }
            }

            _logger.LogDebug("WebsocketClientOrganizerHostedService stopped");
        }
    }
}