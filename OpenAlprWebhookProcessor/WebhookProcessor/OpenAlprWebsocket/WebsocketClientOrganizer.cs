using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class WebsocketClientOrganizer : BackgroundService
    {
        private readonly ConcurrentDictionary<string, OpenAlprWebsocketClient> _connectedClients;

        public WebsocketClientOrganizer()
        {
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

        public async Task<AgentStatusResponse> GetAgentStatusAsync(
            string agentId,
            CancellationToken cancellationToken)
        {
            var agentExists = _connectedClients.TryGetValue(agentId, out var webSocketClient);

            if (!agentExists)
            {
                throw new ArgumentException("AgentId is not connected.");
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

            throw new TimeoutException("Agent did not respond to request.");
        }
    }
}
