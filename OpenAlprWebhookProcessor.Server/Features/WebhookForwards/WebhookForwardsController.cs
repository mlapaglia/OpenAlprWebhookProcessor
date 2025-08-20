using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.Features.WebhookForwards.Queries.GetWebhookForwards;
using OpenAlprWebhookProcessor.Features.WebhookForwards.Commands.UpsertWebhookForwards;

namespace OpenAlprWebhookProcessor.Features.WebhookForwards
{
    [Authorize]
    [ApiController]
    [Route("/api/webhookforwards")]
    public class WebhookForwardsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WebhookForwardsController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
} 