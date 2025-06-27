using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.Settings;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.Settings.GetAgentStatus
{
    public class GetAgentStatusRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public GetAgentStatusRequestHandler(
            ProcessorContext processorContext,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _processorContext = processorContext;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<AgentStatus> HandleAsync(CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (agent.Uid == null)
            {
                return new AgentStatus()
                {
                    IsConnected = false,
                };
            }

            var agentStatus = await _websocketClientOrganizer.GetAgentStatusAsync(agent.Uid, cancellationToken);

            if (agentStatus == null)
            {
                return new AgentStatus()
                {
                    IsConnected = false,
                    LastHeartbeatEpochMs = agent.LastHeartbeatEpochMs,
                };
            }

            return new AgentStatus()
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
