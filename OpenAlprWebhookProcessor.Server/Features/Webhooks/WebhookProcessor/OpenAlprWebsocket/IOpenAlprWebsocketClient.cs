using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket
{
    public interface IOpenAlprWebsocketClient
    {
        Task CloseConnectionAsync(CancellationToken cancellationToken = default);

        Task ConsumeMessagesAsync(CancellationToken cancellationToken = default);

        Task SendAgentStartStopRequestAsync(
            Guid transactionId,
            AgentStartStopType startStopType,
            string agentId,
            CancellationToken cancellationToken = default);

        Task SendGetAgentStatusRequestAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default);

        Task SendGetImageRequestAsync(
            Guid transactionId,
            long cameraId,
            string imageUuid,
            CancellationToken cancellationToken = default);

        Task SendSaveMaskRequestAsync(
            Guid transactionId,
            string maskImage,
            string openAlprName,
            CancellationToken cancellationToken = default);

        bool TryGetAgentResponse<T>(
            Guid transactionId,
            out T agentStatusResponse);

        bool TryGetImageDownloadResponse(
            Guid transactionId,
            out Stream imageDownloadResponse);
    }
}