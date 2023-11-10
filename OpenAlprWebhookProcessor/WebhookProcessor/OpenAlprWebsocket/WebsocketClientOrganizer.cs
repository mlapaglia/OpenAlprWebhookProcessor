using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Cameras.UpsertMasks;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class WebsocketClientOrganizer : BackgroundService
    {
        private readonly ConcurrentDictionary<string, OpenAlprWebsocketClient> _connectedClients;

        private readonly ILogger<WebsocketClientOrganizer> _logger;

        public WebsocketClientOrganizer(
            ILogger<WebsocketClientOrganizer> logger)
        {
            _logger = logger;
            _connectedClients = new ConcurrentDictionary<string, OpenAlprWebsocketClient>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public bool AddAgent(
            string agentId,
            OpenAlprWebsocketClient webSocketClient)
        {
            var wasUpdated = false;

            _connectedClients.AddOrUpdate(
                agentId,
                webSocketClient, (key, oldValue) =>
                {
                    wasUpdated = true;
                    return webSocketClient;
                });

            return wasUpdated;
        }

        public async Task RemoveAgentAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            if (_connectedClients.TryRemove(agentId, out var webSocketClient))
            {
                await webSocketClient.CloseConnectionAsync(cancellationToken);
            }
        }

        public async Task<Stream> GetCameraImageAsync(
            string agentId,
            long cameraId,
            CancellationToken cancellationToken)
        {
            var agentExists = _connectedClients.TryGetValue(agentId, out var webSocketClient);

            if (!agentExists)
            {
                _logger.LogError("AgentId is not connected: {agentId}", agentId);
                return null;
            }

            var transactionId = Guid.NewGuid();

            await webSocketClient.SendGetImageRequestAsync(
                transactionId,
                cameraId,
                cancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetImageDownloadResponse(transactionId, out var imageDownloadResponse))
                {
                    return imageDownloadResponse;
                }

                await Task.Delay(1000, cancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return null;
        }

        public async Task<AgentStatusResponse> GetAgentStatusAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            var agentExists = _connectedClients.TryGetValue(agentId, out var webSocketClient);

            if (!agentExists)
            {
                _logger.LogError("AgentId is not connected: {agentId}", agentId);
                return null;
            }

            var transactionId = Guid.NewGuid();

            await webSocketClient.SendGetAgentStatusRequestAsync(transactionId, cancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetAgentStatusResponse(transactionId, out var agentStatusResponse))
                {
                    return agentStatusResponse;
                }

                await Task.Delay(1000, cancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return null;
        }

        public async Task<bool> DisableAgentAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            var agentExists = _connectedClients.TryGetValue(agentId, out var webSocketClient);

            if (!agentExists)
            {
                _logger.LogError("AgentId is not connected: {agentId}", agentId);
                return false;
            }

            var transactionId = Guid.NewGuid();

            await webSocketClient.SendAgentStartStopRequestAsync(
                transactionId,
                AgentStartStopType.Stop,
                agentId,
                cancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetAgentStatusResponse(transactionId, out var agentStatusResponse))
                {
                    return true;
                }

                await Task.Delay(1000, cancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return false;
        }

        public async Task<bool> UpsertCameraMaskAsync(
            string agentId,
            string configFilename,
            string maskImage,
            CancellationToken cancellationToken)
        {
            var agentExists = _connectedClients.TryGetValue(agentId, out var webSocketClient);

            if (!agentExists)
            {
                _logger.LogError("AgentId is not connected: {agentId}", agentId);
                return false;
            }

            var transactionId = Guid.NewGuid();

            await webSocketClient.SendSaveMaskRequestAsync(
                transactionId,
                maskImage,
                configFilename,
                cancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetAgentStatusResponse(transactionId, out var agentStatusResponse))
                {
                    return true;
                }

                await Task.Delay(1000, cancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return false;
        }
    }
}
