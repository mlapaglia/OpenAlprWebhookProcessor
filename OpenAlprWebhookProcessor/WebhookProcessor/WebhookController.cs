using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly bool _webRequestLoggingEnabled;

        private readonly ILogger<WebhookController> _logger;

        private readonly WebhookHandler _webhookHandler;

        public WebhookController(
            IConfiguration configuration,
            ILogger<WebhookController> logger,
            WebhookHandler webhookHandler)
        {
            _webRequestLoggingEnabled = configuration.GetValue("WebRequestLoggingEnabled", false);
            _logger = logger;
            _webhookHandler = webhookHandler;
        }

        [HttpPost]
        public async Task Post()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var jsonString = await reader.ReadToEndAsync();

                if (_webRequestLoggingEnabled)
                {
                    _logger.LogInformation("request received {0}", jsonString);
                }

                Webhook webhook;

                if (jsonString.Contains("alpr_alert"))
                {
                    webhook = JsonSerializer.Deserialize<Webhook>(jsonString);
                }
                else
                {
                    webhook = new Webhook
                    {
                        Group = JsonSerializer.Deserialize<Group>(jsonString)
                    };
                }

                _logger.LogInformation("request received from: " + Request.HttpContext.Connection.RemoteIpAddress);
                _webhookHandler.HandleWebhook(webhook);
            }
        }

        [HttpGet]
        public ActionResult Get()
        {
            _logger.LogInformation("test succedded from: " + Request.HttpContext.Connection.RemoteIpAddress);
            return Ok("Webhook Processor");
        }
    }
}
