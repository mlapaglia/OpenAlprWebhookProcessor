using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;

        private readonly GroupWebhookHandler _groupWebhookHandler;

        private readonly SinglePlateWebhookHandler _singlePlateWebhookHandler;

        private readonly ProcessorContext _processorContext;

        public WebhookController(
            ILogger<WebhookController> logger,
            GroupWebhookHandler webhookHandler,
            SinglePlateWebhookHandler singlePlateWebhookHandler,
            ProcessorContext processorContext)
        {
            _logger = logger;
            _groupWebhookHandler = webhookHandler;
            _singlePlateWebhookHandler = singlePlateWebhookHandler;
            _processorContext = processorContext;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CancellationToken cancellationToken)
        {
            _logger.LogInformation("request received from: {ipAddress}", Request.HttpContext.Connection.RemoteIpAddress);

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var rawWebhook = await reader.ReadToEndAsync(cancellationToken);

                if (rawWebhook.Contains("alpr_alert"))
                {
                    _logger.LogInformation("parsing alert webhook");
                    var alertGroupResult = JsonSerializer.Deserialize<Webhook>(rawWebhook);

                    await _groupWebhookHandler.HandleWebhookAsync(
                    alertGroupResult,
                    false,
                    cancellationToken);
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

                    await _groupWebhookHandler.HandleWebhookAsync(
                        parsedWebhook,
                        false,
                        cancellationToken);
                }
                else if (rawWebhook.Contains("alpr_results"))
                {
                    _logger.LogInformation("parsing single webhook");
                    var singlePlateResult = JsonSerializer.Deserialize<SinglePlate>(rawWebhook);

                    await _singlePlateWebhookHandler.HandleWebhookAsync(
                        singlePlateResult,
                        cancellationToken);
                }
                else if (rawWebhook.Contains("openalpr_webhook\": \"test"))
                {
                    return Ok("Test successful");
                }
                else if (rawWebhook.Contains("heartbeat"))
                {
                    _logger.LogInformation("received heartbeat from agent");

                    var agent = await _processorContext.Agents
                        .FirstOrDefaultAsync(cancellationToken);

                    agent.LastHeartbeatEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    await _processorContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    _logger.LogInformation("Unknown payload received, ignoring: {rawWebhook}", rawWebhook);
                }
            }

            return Ok();
        }

        [HttpGet]
        public ActionResult Get()
        {
            _logger.LogInformation("test succeeded from: {remoteIpAddress}", Request.HttpContext.Connection.RemoteIpAddress);
            return Ok("Webhook Processor");
        }
    }
}
