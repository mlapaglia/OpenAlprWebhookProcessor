using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Settings.AgentHydration;
using OpenAlprWebhookProcessor.Settings.GetIgnores;
using OpenAlprWebhookProcessor.Settings.Tags;
using OpenAlprWebhookProcessor.Settings.UpdatedCameras;
using OpenAlprWebhookProcessor.Settings.UpsertWebhookForwards;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    [Authorize]
    [ApiController]
    [Route("settings")]
    public class SettingsController : ControllerBase
    {
        private readonly GetAgentRequestHandler _getAgentRequestHandler;

        private readonly UpsertAgentRequestHandler _upsertAgentRequestHandler;

        private readonly GetIgnoresRequestHandler _getIgnoresRequestHandler;

        private readonly UpsertIgnoresRequestHandler _upsertIgnoresRequestHandler;

        private readonly GetWebhookForwardsRequestHandler _getWebhookForwardsRequestHandler;

        private readonly UpsertWebhookForwardsRequestHandler _upsertWebhookForwardsRequestHandler;

        private readonly AgentScrapeRequestHandler _agentHydrationRequestHandler;

        private readonly GetTagsRequestHandler _getTagsRequestHandler;

        private readonly UpsertTagsRequestHandler _upsertTagsRequestHandler;

        public SettingsController(
            GetAgentRequestHandler getAgentRequestHandler,
            UpsertAgentRequestHandler upsertAgentRequestHandler,
            GetIgnoresRequestHandler getIgnoresRequestHandler,
            UpsertIgnoresRequestHandler upsertIgnoresRequestHandler,
            GetWebhookForwardsRequestHandler getWebhookForwardsRequestHandler,
            UpsertWebhookForwardsRequestHandler upsertWebhookForwardsRequestHandler,
            AgentScrapeRequestHandler agentHydrationRequestHandler,
            GetTagsRequestHandler getTagsRequestHandler,
            UpsertTagsRequestHandler upsertTagsRequestHandler)
        {
            _getAgentRequestHandler = getAgentRequestHandler;
            _upsertAgentRequestHandler = upsertAgentRequestHandler;
            _getIgnoresRequestHandler = getIgnoresRequestHandler;
            _upsertIgnoresRequestHandler = upsertIgnoresRequestHandler;
            _getWebhookForwardsRequestHandler = getWebhookForwardsRequestHandler;
            _upsertWebhookForwardsRequestHandler = upsertWebhookForwardsRequestHandler;
            _agentHydrationRequestHandler = agentHydrationRequestHandler;
            _getTagsRequestHandler = getTagsRequestHandler;
            _upsertTagsRequestHandler = upsertTagsRequestHandler;
        }

        [HttpGet("agent")]
        public async Task<Agent> GetAgent(CancellationToken cancellationToken)
        {
            return await _getAgentRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpPost("agent")]
        public async Task UpsertAgent([FromBody] Agent agent)
        {
            await _upsertAgentRequestHandler.HandleAsync(agent);
        }


        [HttpPost("agent/scrape")]
        public IActionResult StartScrape()
        {
            _agentHydrationRequestHandler.Handle();

            return StatusCode(202);
        }

        [HttpPost("ignores/add")]
        public async Task AddIgnore([FromBody] Ignore ignore)
        {
            await _upsertIgnoresRequestHandler.AddIgnoreAsync(ignore);
        }

        [HttpPost("ignores")]
        public async Task UpsertIgnore([FromBody] List<Ignore> ignores)
        {
            await _upsertIgnoresRequestHandler.UpsertIgnoresAsync(ignores);
        }

        [HttpGet("ignores")]
        public async Task<List<Ignore>> GetIgnores()
        {
            return await _getIgnoresRequestHandler.HandleAsync();
        }

        [HttpGet("forwards")]
        public async Task<List<WebhookForward>> GetForwards(CancellationToken cancellationToken)
        {
            return await _getWebhookForwardsRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpPost("forwards")]
        public async Task UpsertForwards([FromBody] List<WebhookForward> ignores)
        {
            await _upsertWebhookForwardsRequestHandler.HandleAsync(ignores);
        }

        [HttpGet("tags")]
        public async Task<List<Tag>> GetTags(CancellationToken cancellationToken)
        {
            return await _getTagsRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpPost("tags")]
        public async Task UpsertTags([FromBody] List<Tag> tags)
        {
            await _upsertTagsRequestHandler.HandleAsync(tags);
        }
    }
}
