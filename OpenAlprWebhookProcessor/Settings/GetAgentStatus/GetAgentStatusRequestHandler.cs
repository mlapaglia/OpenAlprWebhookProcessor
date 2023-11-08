using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
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
            var agentUid = await _processorContext.Agents
                .AsNoTracking()
                .Select(x => x.Uid)
                .FirstOrDefaultAsync(cancellationToken);

            if (agentUid == null)
            {
                return new AgentStatus()
                {
                    IsConnected = false,
                };
            }

            var agentStatus = await _websocketClientOrganizer.GetAgentStatusAsync(agentUid, cancellationToken);

            if (agentStatus == null)
            {
                return new AgentStatus()
                {
                    IsConnected = false,
                };
            }

            return new AgentStatus()
            {
                AgentEpochMs = agentStatus.AgentEpochMs,
                AlprdActive = agentStatus.AgentStatus.AlprdActive,
                Hostname = agentStatus.AgentStatus.AgentHostname,
                IsConnected = true,
                CpuCores = agentStatus.AgentStatus.CpuCores,
                CpuUsagePercent = agentStatus.AgentStatus.CpuUsagePercent,
                DaemonUptimeSeconds = agentStatus.AgentStatus.DaemonUptimeSeconds,
                DiskFreeBytes = agentStatus.AgentStatus.DiskDriveFreeBytes,
                Version = agentStatus.Version,
                SystemUptimeSeconds = agentStatus.AgentStatus.SystemUptimeSeconds,
            };
        }
    }
}
