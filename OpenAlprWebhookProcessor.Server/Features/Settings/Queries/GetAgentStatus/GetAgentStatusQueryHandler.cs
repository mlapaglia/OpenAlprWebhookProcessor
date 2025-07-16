using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus
{
    public class GetAgentStatusQueryHandler : IRequestHandler<GetAgentStatusQuery, AgentStatusDto>
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

        public async Task<AgentStatusDto> Handle(GetAgentStatusQuery request, CancellationToken cancellationToken)
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