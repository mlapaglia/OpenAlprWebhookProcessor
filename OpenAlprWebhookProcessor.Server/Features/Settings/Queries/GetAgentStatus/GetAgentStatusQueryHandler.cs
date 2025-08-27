using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus
{
    public class GetAgentStatusQueryHandler : IQueryHandler<GetAgentStatusQuery, AgentStatusDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;

        public GetAgentStatusQueryHandler(
            IUnitOfWork unitOfWork,
            IWebsocketClientOrganizer websocketClientOrganizer)
        {
            _unitOfWork = unitOfWork;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async ValueTask<AgentStatusDto> Handle(GetAgentStatusQuery request, CancellationToken cancellationToken)
        {
            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (agent == null || agent.Uid == null)
            {
                return new AgentStatusDto()
                {
                    IsConnected = false,
                };
            }

            var agentStatus = await _websocketClientOrganizer.GetAgentStatusAsync(agent.Uid, cancellationToken);

            if (agentStatus == null)
            {
                return new AgentStatusDto()
                {
                    IsConnected = false,
                    LastHeartbeatEpochMs = agent.LastHeartbeatEpochMs,
                };
            }

            return new AgentStatusDto()
            {
                AgentEpochMs = agentStatus.AgentEpochMs,
                AlprdActive = agentStatus.AgentStatus.AlprdActive,
                CpuCores = agentStatus.AgentStatus.CpuCores,
                CpuUsagePercent = agentStatus.AgentStatus.CpuUsagePercent,
                DaemonUptimeSeconds = agentStatus.AgentStatus.DaemonUptimeSeconds,
                DiskFreeBytes = agentStatus.AgentStatus.DiskDriveFreeBytes,
                Hostname = agentStatus.AgentStatus.AgentHostname,
                IsConnected = true,
                LastHeartbeatEpochMs = agent.LastHeartbeatEpochMs,
                SystemUptimeSeconds = agentStatus.AgentStatus.SystemUptimeSeconds,
                Version = agentStatus.Version,
            };
        }
    }
} 