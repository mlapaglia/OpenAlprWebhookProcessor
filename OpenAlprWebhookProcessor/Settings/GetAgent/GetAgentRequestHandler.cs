using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    public class GetAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetAgentRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<Agent> HandleAsync(CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (agent == null)
            {
                return new Agent();
            }

            return new Agent()
            {
                EndpointUrl = agent.EndpointUrl,
                Hostname = agent.Hostname,
                IsDebugEnabled = agent.IsDebugEnabled,
                IsImageCompressionEnabled = agent.IsImageCompressionEnabled,
                Latitude = agent.Latitude,
                Longitude = agent.Longitude,
                OpenAlprWebServerApiKey = agent.OpenAlprWebServerApiKey,
                OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                SunriseOffset = agent.SunriseOffset,
                SunsetOffset = agent.SunsetOffset,
                TimezoneOffset = agent.TimeZoneOffset,
                Uid = agent.Uid,
                Version = agent.Version,
            };
        }
    }
}
