using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System;
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
                Id = agent.Id,
                IsDebugEnabled = agent.IsDebugEnabled,
                IsImageCompressionEnabled = agent.IsImageCompressionEnabled,
                LastHeartbeatEpochMs = agent.LastHeartbeatEpochMs,
                Latitude = agent.Latitude,
                Longitude = agent.Longitude,
                OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                NextScrapeInMinutes = agent.NextScrapeEpochMs.HasValue ? Convert.ToInt32(Math.Floor((DateTimeOffset.FromUnixTimeMilliseconds(agent.NextScrapeEpochMs.Value) - DateTimeOffset.UtcNow).TotalMinutes)) : null,
                ScheduledScrapingIntervalMinutes = agent.ScheduledScrapingIntervalMinutes,
                SunriseOffset = agent.SunriseOffset,
                SunsetOffset = agent.SunsetOffset,
                TimezoneOffset = agent.TimeZoneOffset,
                Uid = agent.Uid,
            };
        }
    }
}
