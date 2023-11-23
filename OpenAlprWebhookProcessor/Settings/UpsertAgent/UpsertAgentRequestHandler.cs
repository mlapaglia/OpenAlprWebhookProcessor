using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Hydrator;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    public class UpsertAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly ImageRetrieverService _imageRetrieverService;

        private readonly HydrationService _hydrationService;

        public UpsertAgentRequestHandler(
            ProcessorContext processorContext,
            ImageRetrieverService imageRetrieverService,
            HydrationService hydrationService)
        {
            _processorContext = processorContext;
            _imageRetrieverService = imageRetrieverService;
            _hydrationService = hydrationService;
        }

        public async Task HandleAsync(Agent agent)
        {
            var dbAgent = await _processorContext.Agents
                .FirstOrDefaultAsync();

            var wasImageCompressionEnabled = false;

            if (agent.IsImageCompressionEnabled && !dbAgent.IsImageCompressionEnabled)
            {
                wasImageCompressionEnabled = true;
            }

            if (dbAgent == null)
            {
                dbAgent = new Data.Agent()
                {
                    EndpointUrl = agent.EndpointUrl,
                    IsDebugEnabled = agent.IsDebugEnabled,
                    IsImageCompressionEnabled = agent.IsImageCompressionEnabled,
                    Latitude = agent.Latitude,
                    Longitude = agent.Longitude,
                    OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                    ScheduledScrapingIntervalMinutes = agent.ScheduledScrapingIntervalMinutes,
                    SunriseOffset = agent.SunriseOffset,
                    SunsetOffset = agent.SunsetOffset,
                    TimeZoneOffset = agent.TimezoneOffset,
                    Uid = agent.Uid,
                };

                _processorContext.Add(dbAgent);
            }
            else
            {
                dbAgent.EndpointUrl = agent.EndpointUrl;
                dbAgent.IsDebugEnabled = agent.IsDebugEnabled;
                dbAgent.IsImageCompressionEnabled = agent.IsImageCompressionEnabled;
                dbAgent.Latitude = agent.Latitude;
                dbAgent.Longitude = agent.Longitude;
                dbAgent.OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl;
                dbAgent.ScheduledScrapingIntervalMinutes = agent.ScheduledScrapingIntervalMinutes;
                dbAgent.SunsetOffset = agent.SunsetOffset;
                dbAgent.SunriseOffset = agent.SunriseOffset;
                dbAgent.TimeZoneOffset = agent.TimezoneOffset;
                dbAgent.Uid = agent.Uid;
            }

            await _processorContext.SaveChangesAsync();

            if (wasImageCompressionEnabled)
            {
                _imageRetrieverService.AddImageCompressionJob();
            }

            if (agent.ScheduledScrapingIntervalMinutes != null)
            {
                await _hydrationService.ScheduleHydrationAsync(default);
            }
        }
    }
}
