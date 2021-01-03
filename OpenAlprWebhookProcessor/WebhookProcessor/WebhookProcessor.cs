using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    [ApiController]
    [Route("webhook")]
    public class WebhookProcessor : ControllerBase
    {
        private readonly ILogger<WebhookProcessor> _logger;

        private readonly WebhookHandler _webhookHandler;

        public WebhookProcessor(
            ILogger<WebhookProcessor> logger,
            WebhookHandler webhookHandler)
        {
            _logger = logger;
            _webhookHandler = webhookHandler;
        }

        [HttpPost]
        public void Post([FromBody] OpenAlprWebhook webhookEvent)
        {
            _logger.LogInformation("request received from: " + Request.HttpContext.Connection.RemoteIpAddress);
            _webhookHandler.Handle(webhookEvent);
        }

        [HttpGet]
        public ActionResult Get()
        {
            _logger.LogInformation("test succedded from: " + Request.HttpContext.Connection.RemoteIpAddress);
            return Ok("Webhook Processor");
        }
    }
}
