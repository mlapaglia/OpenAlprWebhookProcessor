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
using System.IO;
using System.Collections;

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

            var inFlightResponse = string.Empty;

            while (!receiveResult.CloseStatus.HasValue)
            {
                inFlightResponse += Encoding.UTF8.GetString(buffer.ToArray(), 0, Array.FindLastIndex(buffer, b => b != 0) + 1);

                if (receiveResult.EndOfMessage)
                {
                    var transactionMatch = TransactionIdRegex().Match(inFlightResponse);

                    if (transactionMatch.Success)
                    {
                        _availableResponses.TryAdd(Guid.Parse(transactionMatch.Groups[1].Value), inFlightResponse);
                    }
                    else
                    {
                        _logger.LogError("End of message but no transaction id found {response}", inFlightResponse);
                    }

                    inFlightResponse = string.Empty;
                }

                Array.Clear(buffer, 0, buffer.Length);

                receiveResult = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);
            }
        }

        public async Task SendGetImageRequestAsync(
            Guid transactionId,
            long cameraId,
            CancellationToken cancellationToken)
        {
            var imageRequest = new ImageDownloadRequest()
            {
                CameraId = cameraId,
                Direction = "request",
                RequestType = RequestType.GetRequestType(OpenAlprRequestType.ImageDownload),
                TransactionId = transactionId,
            };

            var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(imageRequest));

            await _webSocket.SendAsync(
                new ArraySegment<byte>(message, 0, message.Length),
                WebSocketMessageType.Binary,
                true,
                cancellationToken);
        }

        public bool TryGetImageDownloadResponse(
            Guid transactionId,
            out Stream imageDownloadResponse)
        {
            if (_availableResponses.TryRemove(transactionId, out string message))
            {
                try
                {
                    imageDownloadResponse = new MemoryStream(
                        Encoding.UTF8.GetBytes(
                            JsonSerializer.Deserialize<ImageDownloadResponse>(message).Image));

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize AgentStatusResponse");
                    imageDownloadResponse = null;
                    return false;
                }
            }

            imageDownloadResponse = null;
            return false;
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
                RequestType = RequestType.GetRequestType(OpenAlprRequestType.AgentStatus),
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

        public async Task SendSaveMaskRequestAsync(
            Guid transactionId,
            string maskImage,
            string filename,
            CancellationToken cancellationToken)
        {
            var saveMaskRequest = new ConfigSaveMaskRequest()
            {
                MaskImage = maskImage,
                RequestType = RequestType.GetRequestType(OpenAlprRequestType.ConfigSaveMask),
                StreamFile = "door",
                TransactionId = transactionId,
                Direction = "request",
            };

            var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(saveMaskRequest));

            await _webSocket.SendAsync(
                new ArraySegment<byte>(message, 0, message.Length),
                WebSocketMessageType.Binary,
                true,
                cancellationToken);
        }

        public async Task SendAgentStartStopRequestAsync(
            Guid transactionId,
            AgentStartStopType startStopType,
            string agentId,
            CancellationToken cancellationToken)
        {
            var saveMaskRequest = new AgentStartStopRequest()
            {
                AgentId = agentId,
                AgentOp = startStopType.ToString(),
                Direction = "request",
                RequestType = RequestType.GetRequestType(OpenAlprRequestType.ConfigAgentOperation),
                TransactionId = transactionId,
            };

            var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(saveMaskRequest));

            await _webSocket.SendAsync(
                new ArraySegment<byte>(message, 0, message.Length),
                WebSocketMessageType.Binary,
                true,
                cancellationToken);
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
