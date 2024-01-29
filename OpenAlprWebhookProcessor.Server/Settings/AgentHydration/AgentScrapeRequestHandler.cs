using OpenAlprWebhookProcessor.Hydrator;

namespace OpenAlprWebhookProcessor.Settings.AgentHydration
{
    public class AgentScrapeRequestHandler
    {
        private readonly HydrationService _hydrationService;

        public AgentScrapeRequestHandler(HydrationService hydrationService)
        {
            _hydrationService = hydrationService;
        }

        public void Handle()
        {
            _hydrationService.StartHydration("hydration");
        }
    }
}
