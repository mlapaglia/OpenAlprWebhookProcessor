using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public partial class OpenAlprWebsocketClient
    {
        private readonly ConcurrentDictionary<Guid, string> _availableResponses;

        private WebSocket _webSocket;

        private readonly string _agentId;

        private readonly ILogger _logger;

        public OpenAlprWebsocketClient(
            ILogger logger,
            string agentId,
            WebSocket webSocket)
        {
            _logger = logger;
            _agentId = agentId;
            _webSocket = webSocket;
            _availableResponses = new ConcurrentDictionary<Guid, string>();
        }

        public async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[4096 * 4];
            var receiveResult = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), cancellationToken);

            while (!receiveResult.CloseStatus.HasValue)
            {
                var rawMessage = Encoding.UTF8.GetString(buffer.ToArray(), 0, Array.FindLastIndex(buffer, b => b != 0) + 1);

                var transactionMatch = TransactionIdRegex().Match(rawMessage);

                if (transactionMatch.Success)
                {
                    _availableResponses.TryAdd(Guid.Parse(transactionMatch.Groups[1].Value), rawMessage);
                }

                Array.Clear(buffer, 0, buffer.Length);

                receiveResult = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);
            }
        }

        public async Task SendGetAgentStatusRequestAsync(
            Guid transactionId,
            CancellationToken cancellationToken)
        {
            var agentStatusRequest = new ConfigInfoRequest()
            {
                AgentId = _agentId,
                Direction = "request",
                TransactionId = transactionId.ToString(),
                RequestType = RequestType.GetRequestType(OpenAlprRequestType.agent_status),
                Version = 1,
            };

            var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(agentStatusRequest));

            await _webSocket.SendAsync(
                new ArraySegment<byte>(message, 0, message.Length),
                WebSocketMessageType.Binary,
                true,
                cancellationToken);
        }

        public bool TryGetAgentStatusResponse(
            Guid transactionId,
            out AgentStatusResponse agentStatusResponse)
        {
            if (_availableResponses.TryRemove(transactionId, out string message))
            {
                try
                {
                    agentStatusResponse = JsonSerializer.Deserialize<AgentStatusResponse>(message);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize AgentStatusResponse");
                    agentStatusResponse = null;
                    return false;
                }
            }

            agentStatusResponse = null;
            return false;
        }

        public async Task CloseConnectionAsync(CancellationToken cancellationToken)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "goodbye.",
                    cancellationToken);
            }
        }

        [GeneratedRegex("transaction_id\":\"(.*?)\"")]
        private static partial Regex TransactionIdRegex();
    }
}
