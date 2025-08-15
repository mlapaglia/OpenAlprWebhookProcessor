using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessAlertWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessHeartbeatWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessPlateGroupWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessSinglePlateWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Queries.GetWebhookStatus;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IMediator _mediator;

        public WebhookController(
            ILogger<WebhookController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CancellationToken cancellationToken)
        {
            _logger.LogInformation("request received from: {IpAddress}", Request.HttpContext.Connection.RemoteIpAddress);

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var rawWebhook = await reader.ReadToEndAsync(cancellationToken);

                if (rawWebhook.Contains("alpr_alert"))
                {
                    _logger.LogInformation("parsing alert webhook");
                    var alertGroupResult = JsonSerializer.Deserialize<Webhook>(rawWebhook);
                    var command = new ProcessAlertWebhookCommand(alertGroupResult, false);
                    await _mediator.Send(command, cancellationToken);
                }
                else if (rawWebhook.Contains("alpr_group"))
                {
                    _logger.LogInformation("parsing plate group webhook");
                    Webhook parsedWebhook = new();

                    if (rawWebhook.Contains("\"data_type\":null"))
                    {
                        parsedWebhook = JsonSerializer.Deserialize<Webhook>(rawWebhook);
                    }
                    else
                    {
                        parsedWebhook.Group = JsonSerializer.Deserialize<Group>(rawWebhook);
                    }

                    var command = new ProcessPlateGroupWebhookCommand(parsedWebhook, false);
                    await _mediator.Send(command, cancellationToken);
                }
                else if (rawWebhook.Contains("alpr_results"))
                {
                    _logger.LogInformation("parsing single webhook");
                    var singlePlateResult = JsonSerializer.Deserialize<SinglePlate>(rawWebhook);
                    var command = new ProcessSinglePlateWebhookCommand(singlePlateResult);
                    await _mediator.Send(command, cancellationToken);
                }
                else if (rawWebhook.Contains("openalpr_webhook\": \"test"))
                {
                    return Ok("Test successful");
                }
                else if (rawWebhook.Contains("heartbeat"))
                {
                    _logger.LogInformation("received heartbeat from agent");
                    var command = new ProcessHeartbeatWebhookCommand();
                    await _mediator.Send(command, cancellationToken);
                }
                else
                {
                    _logger.LogDebug("Unknown payload received, ignoring: {RawWebhook}", rawWebhook);
                }
            }

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> Get(CancellationToken cancellationToken)
        {
            _logger.LogInformation("test succeeded from: {RemoteIpAddress}", Request.HttpContext.Connection.RemoteIpAddress);
            var query = new GetWebhookStatusQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
} 