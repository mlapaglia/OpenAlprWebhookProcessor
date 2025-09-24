using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket
{
    public interface IWebsocketClientOrganizer
    {
        Task<AddAgentResult> AddAgentAsync(
            string agentId,
            IOpenAlprWebsocketClient webSocketClient,
            CancellationToken cancellationToken = default);

        Task RemoveAgentAsync(
            string agentId,
            CancellationToken cancellationToken = default);

        Task<AgentStatusResponse> GetAgentStatusAsync(
            string agentId,
            CancellationToken cancellationToken = default);

        Task<bool> DisableEnableAgentAsync(
            string agentId,
            AgentStartStopType startStopType,
            CancellationToken cancellationToken = default);

        Task<bool> UpsertCameraMaskAsync(
            string agentId,
            string maskImage,
            string openAlprName,
            CancellationToken cancellationToken = default);

        Task<ImageDownloadResponse> GetCameraSnapshotAsync(
            string agentId,
            long cameraId,
            CancellationToken cancellationToken = default);

        Task DisconnectAllClientsAsync(CancellationToken cancellationToken = default);
    }
}