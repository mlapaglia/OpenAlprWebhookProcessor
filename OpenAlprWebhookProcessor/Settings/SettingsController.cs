using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Settings.GetIgnores;
using OpenAlprWebhookProcessor.Settings.UpdatedCameras;
using System.Collections.Generic;
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

        public SettingsController(
            GetAgentRequestHandler getAgentRequestHandler,
            UpsertAgentRequestHandler upsertAgentRequestHandler,
            GetIgnoresRequestHandler getIgnoresRequestHandler,
            UpsertIgnoresRequestHandler upsertIgnoresRequestHandler)
        {
            _getAgentRequestHandler = getAgentRequestHandler;
            _upsertAgentRequestHandler = upsertAgentRequestHandler;
            _getIgnoresRequestHandler = getIgnoresRequestHandler;
            _upsertIgnoresRequestHandler = upsertIgnoresRequestHandler;
        }

        [HttpGet("agent")]
        public async Task<Agent> GetAgent()
        {
            return await _getAgentRequestHandler.HandleAsync();
        }

        [HttpPost("agent")]
        public async Task UpsertAgent([FromBody] Agent agent)
        {
            await _upsertAgentRequestHandler.HandleAsync(agent);
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
    }
}
