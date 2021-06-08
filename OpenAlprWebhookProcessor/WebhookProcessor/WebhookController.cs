using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
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

        public WebhookController(
            ILogger<WebhookController> logger,
            GroupWebhookHandler webhookHandler,
            SinglePlateWebhookHandler singlePlateWebhookHandler)
        {
            _logger = logger;
            _groupWebhookHandler = webhookHandler;
            _singlePlateWebhookHandler = singlePlateWebhookHandler;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CancellationToken cancellationToken)
        {
            _logger.LogInformation("request received from: " + Request.HttpContext.Connection.RemoteIpAddress);

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var rawWebhook = await reader.ReadToEndAsync();

                if (rawWebhook.Contains("alpr_alert"))
                {
                    _logger.LogInformation("parsing alert webhook");
                    var alertGroupResult = JsonSerializer.Deserialize<Webhook>(rawWebhook);

                    await _groupWebhookHandler.HandleWebhookAsync(
                    alertGroupResult,
                    cancellationToken);
                }
                else if (rawWebhook.Contains("alpr_group"))
                {
                    _logger.LogInformation("parsing plate group webhook");
                    var groupResult = new Webhook
                    {
                        Group = JsonSerializer.Deserialize<Group>(rawWebhook)
                    };

                    await _groupWebhookHandler.HandleWebhookAsync(
                        groupResult,
                        cancellationToken);
                }
                else if (rawWebhook.Contains("\"vehicle\""))
                {
                    _logger.LogInformation("parsing vehicle webhook");
                    var groupResult = new Webhook
                    {
                        Group = JsonSerializer.Deserialize<Group>(rawWebhook)
                    };

                    await _groupWebhookHandler.HandleWebhookAsync(
                        groupResult,
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
                }
                else
                {
                    _logger.LogInformation("Unknown payload received, ignoring: " + rawWebhook);
                }
            }

            return Ok();
        }

        [HttpGet]
        public ActionResult Get()
        {
            _logger.LogInformation("test succeeded from: " + Request.HttpContext.Connection.RemoteIpAddress);
            return Ok("Webhook Processor");
        }
    }
}
