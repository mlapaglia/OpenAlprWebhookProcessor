using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    public class UpsertAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly ImageRetrieverService _imageRetrieverService;

        public UpsertAgentRequestHandler(
            ProcessorContext processorContext,
            ImageRetrieverService imageRetrieverService)
        {
            _processorContext = processorContext;
            _imageRetrieverService = imageRetrieverService;
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
                    Hostname = agent.Hostname,
                    IsDebugEnabled = agent.IsDebugEnabled,
                    IsImageCompressionEnabled = agent.IsImageCompressionEnabled,
                    Latitude = agent.Latitude,
                    Longitude = agent.Longitude,
                    OpenAlprWebServerApiKey = agent.OpenAlprWebServerApiKey,
                    OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                    SunriseOffset = agent.SunriseOffset,
                    SunsetOffset = agent.SunsetOffset,
                    TimeZoneOffset = agent.TimezoneOffset,
                    Uid = agent.Uid,
                    Version = agent.Version,
                };

                _processorContext.Add(dbAgent);
            }
            else
            {
                dbAgent.EndpointUrl = agent.EndpointUrl;
                dbAgent.Hostname = agent.Hostname;
                dbAgent.IsDebugEnabled = agent.IsDebugEnabled;
                dbAgent.IsImageCompressionEnabled = agent.IsImageCompressionEnabled;
                dbAgent.Latitude = agent.Latitude;
                dbAgent.Longitude = agent.Longitude;
                dbAgent.OpenAlprWebServerApiKey = agent.OpenAlprWebServerApiKey;
                dbAgent.OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl;
                dbAgent.SunsetOffset = agent.SunsetOffset;
                dbAgent.SunriseOffset = agent.SunriseOffset;
                dbAgent.TimeZoneOffset = agent.TimezoneOffset;
                dbAgent.Uid = agent.Uid;
                dbAgent.Version = agent.Version;
            }

            await _processorContext.SaveChangesAsync();

            if(wasImageCompressionEnabled)
            {
                _imageRetrieverService.AddImageCompressionJob();
            }
        }
    }
}
