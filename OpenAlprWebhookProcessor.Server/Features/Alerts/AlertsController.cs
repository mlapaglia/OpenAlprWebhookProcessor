using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.TestPushover;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.TestWebPush;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertAlerts;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertPushover;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertWebPush;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetAlerts;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetPushover;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetWebPush;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    [Authorize]
    [ApiController]
    [Route("api/alerts")]
    public class AlertsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AlertsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddAlert([FromBody] Alert alert, CancellationToken cancellationToken)
        {
            var command = new AddAlertCommand(alert);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> UpsertAlerts([FromBody] List<Alert> alerts, CancellationToken cancellationToken)
        {
            var command = new UpsertAlertsCommand(alerts);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<Alert>>> GetAlerts(CancellationToken cancellationToken)
        {
            var query = new GetAlertsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("pushover")]
        public async Task<ActionResult> UpsertPushover([FromBody] PushoverRequest request, CancellationToken cancellationToken)
        {
            var command = new UpsertPushoverCommand(request);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPost("pushover/test")]
        public async Task<ActionResult> TestPushover(CancellationToken cancellationToken)
        {
            var command = new TestPushoverCommand();
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpGet("pushover")]
        public async Task<ActionResult<PushoverRequest>> GetPushover(CancellationToken cancellationToken)
        {
            var query = new GetPushoverQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("webpush")]
        public async Task<ActionResult> UpsertWebpush([FromBody] WebPushRequest request, CancellationToken cancellationToken)
        {
            var command = new UpsertWebPushCommand(request);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPost("webpush/test")]
        public async Task<ActionResult> TestWebpush(CancellationToken cancellationToken)
        {
            var command = new TestWebPushCommand();
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpGet("webpush")]
        public async Task<ActionResult<WebPushRequest>> GetWebpush(CancellationToken cancellationToken)
        {
            var query = new GetWebPushQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
} 