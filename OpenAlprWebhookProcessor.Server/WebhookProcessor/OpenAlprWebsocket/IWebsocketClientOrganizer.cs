using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public interface IWebsocketClientOrganizer
    {
        Task<AddAgentResult> AddAgentAsync(
            string agentId,
            IOpenAlprWebsocketClient webSocketClient,
            CancellationToken cancellationToken);

        Task RemoveAgentAsync(
            string agentId,
            CancellationToken cancellationToken);

        Task<AgentStatusResponse> GetAgentStatusAsync(
            string agentId,
            CancellationToken cancellationToken);

        Task<bool> DisableEnableAgentAsync(
            string agentId,
            AgentStartStopType startStopType,
            CancellationToken cancellationToken);

        Task<bool> UpsertCameraMaskAsync(
            string agentId,
            string maskImage,
            string openAlprName,
            CancellationToken cancellationToken);

        Task DisconnectAllClientsAsync(CancellationToken cancellationToken = default);
    }
}