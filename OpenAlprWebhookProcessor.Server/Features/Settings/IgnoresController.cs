using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AddIgnore;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteIgnore;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpdateIgnore;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings
{
    [Authorize]
    [ApiController]
    [Route("api/ignores")]
    public class IgnoresController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IgnoresController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddIgnore([FromBody] IgnoreDto ignore, CancellationToken cancellationToken)
        {
            var command = new AddIgnoreCommand(ignore);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<IgnoreDto>>> GetIgnores(CancellationToken cancellationToken)
        {
            var query = new GetIgnoresQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateIgnore(Guid id, [FromBody] IgnoreDto ignore, CancellationToken cancellationToken)
        {
            if (id != ignore.Id)
            {
                return BadRequest("Ignore ID in URL does not match ignore ID in body");
            }

            var command = new UpdateIgnoreCommand(ignore);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIgnore(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteIgnoreCommand(id);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
    }
}
