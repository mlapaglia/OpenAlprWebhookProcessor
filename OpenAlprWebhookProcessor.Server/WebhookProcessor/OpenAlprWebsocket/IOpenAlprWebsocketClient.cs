using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebsocket
{
    public interface IOpenAlprWebsocketClient
    {
        Task CloseConnectionAsync(CancellationToken cancellationToken);
        Task ConsumeMessagesAsync(CancellationToken cancellationToken);
        Task SendAgentStartStopRequestAsync(Guid transactionId, AgentStartStopType startStopType, string agentId, CancellationToken cancellationToken);
        Task SendGetAgentStatusRequestAsync(Guid transactionId, CancellationToken cancellationToken);
        Task SendGetImageRequestAsync(Guid transactionId, long cameraId, CancellationToken cancellationToken);
        Task SendSaveMaskRequestAsync(Guid transactionId, string maskImage, string openAlprName, CancellationToken cancellationToken);
        bool TryGetAgentResponse<T>(Guid transactionId, out T agentStatusResponse);
        bool TryGetImageDownloadResponse(Guid transactionId, out Stream imageDownloadResponse);
    }
}