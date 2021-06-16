using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    public class GetAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly OpenAlprAgentScraper _scraper;
        public GetAgentRequestHandler(
            ProcessorContext processorContext,
            OpenAlprAgentScraper scraper)
        {
            _processorContext = processorContext;
            _scraper = scraper;
        }

        public async Task<Agent> HandleAsync(CancellationToken cancellationToken)
        {
            //await _scraper.ScrapeAgentAsync(cancellationToken);
            var agent = await _processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

            if (agent == null)
            {
                return new Agent();
            }

            return new Agent()
            {
                EndpointUrl = agent.EndpointUrl,
                Hostname = agent.Hostname,
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
