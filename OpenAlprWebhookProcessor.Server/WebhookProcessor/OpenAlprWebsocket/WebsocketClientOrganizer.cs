using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class WebsocketClientOrganizer : IWebsocketClientOrganizer
    {
        private readonly ConcurrentDictionary<string, OpenAlprWebsocketClient> _connectedClients = new();

        private readonly ILogger<WebsocketClientOrganizer> _logger;

        private readonly TimeSpan _responseTimeout = TimeSpan.FromSeconds(10);

        public WebsocketClientOrganizer(ILogger<WebsocketClientOrganizer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AddAgentResult> AddAgentAsync(
            string agentId,
            OpenAlprWebsocketClient webSocketClient,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
            ArgumentNullException.ThrowIfNull(webSocketClient);

            var result = new AddAgentResult();

            result.WasUpdated = _connectedClients.TryRemove(agentId, out var oldWebSocketClient);

            if (result.WasUpdated && oldWebSocketClient != null)
            {
                try
                {
                    await oldWebSocketClient.CloseConnectionAsync(cancellationToken);
                    result.UpdateWasCleanDisconnect = true;
                    _logger.LogInformation("Cleanly disconnected old websocket client for agent: {AgentId}", agentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to disconnect old client cleanly for agent: {AgentId}", agentId);
                }
            }

            // Add new client
            if (_connectedClients.TryAdd(agentId, webSocketClient))
            {
                result.WasAdded = true;
                _logger.LogInformation("Added websocket client for agent: {AgentId}", agentId);
            }
            else
            {
                _logger.LogError("Unable to add websocket client for agent: {AgentId}", agentId);
            }

            return result;
        }

        public async Task RemoveAgentAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID cannot be null or empty", nameof(agentId));

            if (_connectedClients.TryRemove(agentId, out var webSocketClient))
            {
                try
                {
                    await webSocketClient.CloseConnectionAsync(cancellationToken);
                    _logger.LogInformation("Removed and disconnected websocket client for agent: {AgentId}", agentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing connection for agent: {AgentId}", agentId);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Agent not found for removal: {AgentId}", agentId);
            }
        }

        public async Task<AgentStatusResponse> GetAgentStatusAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID cannot be null or empty", nameof(agentId));

            if (!_connectedClients.TryGetValue(agentId, out var webSocketClient))
            {
                _logger.LogError("Agent is not connected: {AgentId}", agentId);
                return null;
            }

            var transactionId = Guid.NewGuid();

            try
            {
                await webSocketClient.SendGetAgentStatusRequestAsync(transactionId, cancellationToken);

                var response = await WaitForResponseAsync<AgentStatusResponse>(
                    webSocketClient,
                    transactionId,
                    cancellationToken);

                if (response == null)
                {
                    _logger.LogError("Agent did not respond to status request: {AgentId}", agentId);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for agent: {AgentId}", agentId);
                throw;
            }
        }

        public async Task<bool> DisableEnableAgentAsync(
            string agentId,
            AgentStartStopType startStopType,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID cannot be null or empty", nameof(agentId));

            if (!_connectedClients.TryGetValue(agentId, out var webSocketClient))
            {
                _logger.LogError("Agent is not connected: {AgentId}", agentId);
                return false;
            }

            var transactionId = Guid.NewGuid();

            try
            {
                await webSocketClient.SendAgentStartStopRequestAsync(
                    transactionId,
                    startStopType,
                    agentId,
                    cancellationToken);

                var response = await WaitForResponseAsync<AgentStartStopResponse>(
                    webSocketClient,
                    transactionId,
                    cancellationToken);

                if (response == null)
                {
                    _logger.LogError("Agent did not respond to {StartStopType} request: {AgentId}", startStopType, agentId);
                    return false;
                }

                return response.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending {StartStopType} request to agent: {AgentId}", startStopType, agentId);
                throw;
            }
        }

        public async Task<bool> UpsertCameraMaskAsync(
            string agentId,
            string maskImage,
            string openAlprName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID cannot be null or empty", nameof(agentId));

            if (string.IsNullOrWhiteSpace(maskImage))
                throw new ArgumentException("Mask image cannot be null or empty", nameof(maskImage));

            if (string.IsNullOrWhiteSpace(openAlprName))
                throw new ArgumentException("OpenALPR name cannot be null or empty", nameof(openAlprName));

            if (!_connectedClients.TryGetValue(agentId, out var webSocketClient))
            {
                _logger.LogError("Agent is not connected: {AgentId}", agentId);
                return false;
            }

            var transactionId = Guid.NewGuid();

            try
            {
                await webSocketClient.SendSaveMaskRequestAsync(
                    transactionId,
                    maskImage,
                    openAlprName,
                    cancellationToken);

                var response = await WaitForResponseAsync<AgentStatusResponse>(
                    webSocketClient,
                    transactionId,
                    cancellationToken);

                if (response == null)
                {
                    _logger.LogError("Agent did not respond to save mask request: {AgentId}", agentId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving mask for agent: {AgentId}, camera: {OpenAlprName}", agentId, openAlprName);
                throw;
            }
        }

        public IReadOnlyDictionary<string, OpenAlprWebsocketClient> GetConnectedClients()
        {
            return _connectedClients;
        }

        public async Task DisconnectAllClientsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Disconnecting all {Count} websocket clients", _connectedClients.Count);

            var disconnectTasks = new List<Task>();

            foreach (var kvp in _connectedClients)
            {
                var task = DisconnectClientSafelyAsync(kvp.Key, kvp.Value, cancellationToken);
                disconnectTasks.Add(task);
            }

            await Task.WhenAll(disconnectTasks);
            _connectedClients.Clear();
        }

        private async Task DisconnectClientSafelyAsync(
            string agentId,
            OpenAlprWebsocketClient client,
            CancellationToken cancellationToken)
        {
            try
            {
                await client.CloseConnectionAsync(cancellationToken);
                _logger.LogInformation("Disconnected websocket client for agent: {AgentId}", agentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting websocket client for agent: {AgentId}", agentId);
            }
        }

        private async Task<T> WaitForResponseAsync<T>(
            OpenAlprWebsocketClient webSocketClient,
            Guid transactionId,
            CancellationToken cancellationToken) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < _responseTimeout)
            {
                if (webSocketClient.TryGetAgentResponse<T>(transactionId, out var response))
                {
                    return response;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Response wait cancelled for transaction: {TransactionId}", transactionId);
                    throw;
                }
            }

            return null;
        }
    }
}