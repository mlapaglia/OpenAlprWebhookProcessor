using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public WebsocketClientOrganizer(
            ILogger<WebsocketClientOrganizer> logger)
        {
            _logger = logger;
            _connectedClients = new ConcurrentDictionary<string, OpenAlprWebsocketClient>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                await Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Service cancellation requested, stopping websockets: {message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown error occurred {message}, stopping websockets: {message}", ex.Message);
            }
            finally
            {
                await DisconnectClientsAsync();
            }
        }

        public async Task<AddAgentResult> AddAgentAsync(
            string agentId,
            OpenAlprWebsocketClient webSocketClient,
            CancellationToken cancellationToken)
        {
            var linkedCancellationToken = GetLinkedCancellationToken(cancellationToken);

            var result = new AddAgentResult();

            result.WasUpdated = _connectedClients.TryRemove(agentId, out var oldWebSocketClient);

            if (result.WasUpdated)
            {
                try
                {
                    await oldWebSocketClient.CloseConnectionAsync(linkedCancellationToken);
                    result.UpdateWasCleanDisconnect = true;
                }
                catch
                {
                }
            }

            if (_connectedClients.TryAdd(agentId, webSocketClient))
            {
                result.WasAdded = true;
            }
            else
            {
                _logger.LogError("Unable to add agent: {agentID}", agentId);
            }

            return result;
        }

        public async Task RemoveAgentAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            var linkedCancellationToken = GetLinkedCancellationToken(cancellationToken);

            if (_connectedClients.TryRemove(agentId, out var webSocketClient))
            {
                await webSocketClient.CloseConnectionAsync(linkedCancellationToken);
            }
        }

        public async Task<Stream> GetCameraImageAsync(
            string agentId,
            long cameraId,
            CancellationToken cancellationToken)
        {
            var linkedCancellationToken = GetLinkedCancellationToken(cancellationToken);

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
                linkedCancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetImageDownloadResponse(transactionId, out var imageDownloadResponse))
                {
                    return imageDownloadResponse;
                }

                await Task.Delay(1000, linkedCancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return null;
        }

        public async Task<AgentStatusResponse> GetAgentStatusAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            var linkedCancellationToken = GetLinkedCancellationToken(cancellationToken);

            var agentExists = _connectedClients.TryGetValue(agentId, out var webSocketClient);

            if (!agentExists)
            {
                _logger.LogError("AgentId is not connected: {agentId}", agentId);
                return null;
            }

            var transactionId = Guid.NewGuid();

            await webSocketClient.SendGetAgentStatusRequestAsync(transactionId, linkedCancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetAgentResponse<AgentStatusResponse>(transactionId, out var agentStatusResponse))
                {
                    return agentStatusResponse;
                }

                await Task.Delay(1000, linkedCancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return null;
        }

        public async Task<bool> DisableEnableAgentAsync(
            string agentId,
            AgentStartStopType startStopType,
            CancellationToken cancellationToken)
        {
            var linkedCancellationToken = GetLinkedCancellationToken(cancellationToken);

            var agentExists = _connectedClients.TryGetValue(agentId, out var webSocketClient);

            if (!agentExists)
            {
                _logger.LogError("AgentId is not connected: {agentId}", agentId);
                return false;
            }

            var transactionId = Guid.NewGuid();

            await webSocketClient.SendAgentStartStopRequestAsync(
                transactionId,
                startStopType,
                agentId,
                linkedCancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetAgentResponse<AgentStartStopResponse>(transactionId, out var agentResponse))
                {
                    return agentResponse.Success;
                }

                await Task.Delay(1000, linkedCancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return false;
        }

        public async Task<bool> UpsertCameraMaskAsync(
            string agentId,
            string maskImage,
            string openAlprName,
            CancellationToken cancellationToken)
        {
            var linkedCancellationToken = GetLinkedCancellationToken(cancellationToken);

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
                openAlprName,
                linkedCancellationToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100000)
            {
                if (webSocketClient.TryGetAgentResponse<AgentStatusResponse>(transactionId, out var agentStatusResponse))
                {
                    return true;
                }

                await Task.Delay(1000, linkedCancellationToken);
            }

            _logger.LogError("Agent did not respond to request.");
            return false;
        }

        private CancellationToken GetLinkedCancellationToken(CancellationToken cancellationToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken).Token;
        }

        private async Task DisconnectClientsAsync()
        {
            await Parallel.ForEachAsync(_connectedClients.Values, async (connectedClient, cancellationToken) =>
            {
                await connectedClient.CloseConnectionAsync(default);
            });
        }
    }
}
