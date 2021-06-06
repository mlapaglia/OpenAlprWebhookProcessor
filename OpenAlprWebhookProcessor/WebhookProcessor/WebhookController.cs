using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        private readonly WebhookHandler _webhookHandler;

        public WebhookController(
            ILogger<WebhookController> logger,
            WebhookHandler webhookHandler)
        {
            _logger = logger;
            _webhookHandler = webhookHandler;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CancellationToken cancellationToken)
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var rawWebhook = await reader.ReadToEndAsync();

                Webhook webhook;

                if (rawWebhook.Contains("alpr_alert"))
                {
                    webhook = JsonSerializer.Deserialize<Webhook>(rawWebhook);
                }
                else if (rawWebhook.Contains("openalpr_webhook\": \"test"))
                {
                    return Ok("Test successful");
                }
                else
                {
                    webhook = new Webhook
                    {
                        Group = JsonSerializer.Deserialize<Group>(rawWebhook)
                    };
                }

                _logger.LogInformation("request received from: " + Request.HttpContext.Connection.RemoteIpAddress);

                await _webhookHandler.HandleWebhookAsync(
                    webhook,
                    cancellationToken);
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
