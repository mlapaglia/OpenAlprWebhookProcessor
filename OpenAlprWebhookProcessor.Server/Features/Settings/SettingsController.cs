using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AgentScrape;
using OpenAlprWebhookProcessor.Features.Settings.Commands.CleanupDatabase;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DisableAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.EnableAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.TestEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertEnrichers;

using OpenAlprWebhookProcessor.Features.WebhookForwards.Commands.UpsertWebhookForwards;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentVideoStreams;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers;

using OpenAlprWebhookProcessor.Features.Settings.Queries.GetVersion;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetScheduledJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.Features.WebhookForwards.Queries.GetWebhookForwards;

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

        [HttpGet("agent/video-streams")]
        public async Task<AgentVideoStreamsDto> GetAgentVideoStreams(CancellationToken cancellationToken)
        {
            var query = new GetAgentVideoStreamsQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpPost("agent/disable")]
        public async Task<IActionResult> DisableAgent(Guid agentId, CancellationToken cancellationToken)
        {
            var command = new DisableAgentCommand(agentId);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return BadRequest();
            }

            return Ok(true);
        }

        [HttpPost("agent/enable")]
        public async Task<IActionResult> EnableAgent(Guid agentId, CancellationToken cancellationToken)
        {
            var command = new EnableAgentCommand(agentId);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return BadRequest();
            }

            return Ok(true);
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

        /// <summary>
        /// Used to sanitize the database by removing images, obfuscating plate numbers, and removing
        /// webhook settings and other personally identifiable contents.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("cleanup/database")]
        public async Task<IActionResult> CleanupDatabase(CancellationToken cancellationToken)
        {
            var command = new CleanupDatabaseCommand();
            await _mediator.Send(command, cancellationToken);
            return StatusCode(202);
        }

        [HttpGet("version")]
        public async Task<VersionDto> GetVersion(CancellationToken cancellationToken)
        {
            var query = new GetVersionQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpGet("debug/signalr-connections")]
        public IActionResult GetSignalRConnections()
        {
            var connections = ProcessorHub.ProcessorHub.GetAllConnections();
            var connectionSummary = new
            {
                TotalConnections = connections.Length,
                Connections = connections.Select(c => new
                {
                    c.ConnectionId,
                    c.UserId,
                    c.ConnectedAt,
                    DurationSeconds = (int)(DateTime.UtcNow - c.ConnectedAt).TotalSeconds,
                    c.Transport,
                    c.UserAgent,
                    c.IpAddress
                }).ToArray()
            };
            
            return Ok(connectionSummary);
        }

        [HttpGet("scheduled-jobs")]
        public async Task<ActionResult<GetScheduledJobsResponse>> GetScheduledJobs(CancellationToken cancellationToken)
        {
            var query = new GetScheduledJobsQuery();
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }
    }
} 