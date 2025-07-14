using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AddIgnore;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AgentScrape;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DisableAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.EnableAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.TestEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertIgnores;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertWebhookForwards;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings
{
    [Authorize]
    [ApiController]
    [Route("/api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("agent")]
        public async Task<AgentDto> GetAgent(CancellationToken cancellationToken)
        {
            var query = new GetAgentQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpGet("agent/status")]
        public async Task<AgentStatusDto> GetAgentStatus(CancellationToken cancellationToken)
        {
            var query = new GetAgentStatusQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpPost("agent/disable")]
        public async Task<bool> DisableAgent(Guid agentId, CancellationToken cancellationToken)
        {
            var command = new DisableAgentCommand(agentId);
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpPost("agent/enable")]
        public async Task<bool> EnableAgent(Guid agentId, CancellationToken cancellationToken)
        {
            var command = new EnableAgentCommand(agentId);
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpPost("agent")]
        public async Task UpsertAgent([FromBody] AgentDto agent, CancellationToken cancellationToken)
        {
            var command = new UpsertAgentCommand(agent);
            await _mediator.Send(command, cancellationToken);
        }

        [HttpPost("agent/scrape")]
        public async Task<IActionResult> StartScrape(CancellationToken cancellationToken)
        {
            var command = new AgentScrapeCommand();
            await _mediator.Send(command, cancellationToken);
            return StatusCode(202);
        }

        [HttpPost("ignores/add")]
        public async Task AddIgnore([FromBody] IgnoreDto ignore, CancellationToken cancellationToken)
        {
            var command = new AddIgnoreCommand(ignore);
            await _mediator.Send(command, cancellationToken);
        }

        [HttpPost("ignores")]
        public async Task UpsertIgnore([FromBody] List<IgnoreDto> ignores, CancellationToken cancellationToken)
        {
            var command = new UpsertIgnoresCommand(ignores);
            await _mediator.Send(command, cancellationToken);
        }

        [HttpGet("ignores")]
        public async Task<List<IgnoreDto>> GetIgnores(CancellationToken cancellationToken)
        {
            var query = new GetIgnoresQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpGet("forwards")]
        public async Task<List<WebhookForwardDto>> GetForwards(CancellationToken cancellationToken)
        {
            var query = new GetWebhookForwardsQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpPost("forwards")]
        public async Task UpsertForwards([FromBody] List<WebhookForwardDto> forwards, CancellationToken cancellationToken)
        {
            var command = new UpsertWebhookForwardsCommand(forwards);
            await _mediator.Send(command, cancellationToken);
        }

        [HttpGet("enrichers")]
        public async Task<EnricherDto> GetEnrichers(CancellationToken cancellationToken)
        {
            var query = new GetEnrichersQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpPost("enrichers")]
        public async Task UpsertEnrichers([FromBody] EnricherDto enricher, CancellationToken cancellationToken)
        {
            var command = new UpsertEnrichersCommand(enricher);
            await _mediator.Send(command, cancellationToken);
        }

        [HttpPost("enrichers/{enricherId}/test")]
        public async Task<bool> TestEnrichers(CancellationToken cancellationToken)
        {
            var command = new TestEnrichersCommand();
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpGet("debug/plates")]
        public async Task<ContentResult> GetDebugPlates(
            bool onlyFailedPlateGroups,
            CancellationToken cancellationToken)
        {
            var query = new GetDebugPlatesQuery(onlyFailedPlateGroups);
            var results = await _mediator.Send(query, cancellationToken);
            return Content(results, "application/json");
        }

        [HttpDelete("debug/plates")]
        public async Task DeleteDebugPlates(CancellationToken cancellationToken)
        {
            var command = new DeleteDebugPlatesCommand();
            await _mediator.Send(command, cancellationToken);
        }
    }
} 