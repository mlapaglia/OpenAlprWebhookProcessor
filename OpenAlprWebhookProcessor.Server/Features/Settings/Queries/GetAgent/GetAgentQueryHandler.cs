using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent
{
    public class GetAgentQueryHandler : IQueryHandler<GetAgentQuery, AgentDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAgentQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<AgentDto> Handle(GetAgentQuery request, CancellationToken cancellationToken = default)
        {
            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (agent == null)
            {
                return new AgentDto();
            }

            return new AgentDto
            {
                EndpointUrl = agent.EndpointUrl,
                Id = agent.Id,
                IsDebugEnabled = agent.IsDebugEnabled,
                IsImageCompressionEnabled = agent.IsImageCompressionEnabled,
                LastHeartbeatEpochMs = agent.LastHeartbeatEpochMs,
                Latitude = agent.Latitude,
                Longitude = agent.Longitude,
                OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                NextScrapeInMinutes = agent.NextScrapeEpochMs.HasValue ? 
                    Convert.ToInt32(Math.Floor((DateTimeOffset.FromUnixTimeMilliseconds(agent.NextScrapeEpochMs.Value) - DateTimeOffset.UtcNow).TotalMinutes)) : null,
                ScheduledScrapingIntervalMinutes = agent.ScheduledScrapingIntervalMinutes,
                SunriseOffset = agent.SunriseOffset,
                SunsetOffset = agent.SunsetOffset,
                TimezoneOffset = agent.TimeZoneOffset,
                Uid = agent.Uid,
            };
        }
    }
} 