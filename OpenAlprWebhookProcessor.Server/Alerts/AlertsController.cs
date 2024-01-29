using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    [Authorize]
    [ApiController]
    [Route("api/alerts")]
    public class AlertsController : Controller
    {
        private readonly GetAlertsRequestHandler _getAlertsRequestHandler;

        private readonly UpsertAlertsRequestHandler _upsertAlertsRequestHandler;

        private readonly UpsertPushoverClientRequestHandler _upsertPushoverRequestHandler;

        private readonly GetPushoverClientRequestHandler _getPushoverClientRequestHandler;

        private readonly TestPushoverClientRequestHandler _testPushoverClientRequestHandler;

        private readonly UpsertWebPushClientRequestHandler _upsertWebPushRequestHandler;

        private readonly GetWebPushClientRequestHandler _getWebPushClientRequestHandler;

        private readonly TestWebPushClientRequestHandler _testWebPushClientRequestHandler;

        public AlertsController(
            GetAlertsRequestHandler getAlertsRequestHandler,
            UpsertAlertsRequestHandler upsertAlertsRequestHandler,
            UpsertPushoverClientRequestHandler upsertPushoverRequestHandler,
            GetPushoverClientRequestHandler getPushoverClientRequestHandler,
            TestPushoverClientRequestHandler testPushoverClientRequestHandler,
            UpsertWebPushClientRequestHandler upsertWebPushRequestHandler,
            GetWebPushClientRequestHandler getWebPushClientRequestHandler,
            TestWebPushClientRequestHandler testWebPushClientRequestHandler)
        {
            _getAlertsRequestHandler = getAlertsRequestHandler;
            _upsertAlertsRequestHandler = upsertAlertsRequestHandler;
            _upsertPushoverRequestHandler = upsertPushoverRequestHandler;
            _getPushoverClientRequestHandler = getPushoverClientRequestHandler;
            _testPushoverClientRequestHandler = testPushoverClientRequestHandler;
            _upsertWebPushRequestHandler = upsertWebPushRequestHandler;
            _getWebPushClientRequestHandler = getWebPushClientRequestHandler;
            _testWebPushClientRequestHandler = testWebPushClientRequestHandler;
        }

        [HttpPost("add")]
        public async Task AddAlert([FromBody] Alert alert)
        {
            await _upsertAlertsRequestHandler.AddAlertAsync(alert);
        }

        [HttpPost]
        public async Task UpsertAlerts([FromBody] List<Alert> alerts)
        {
            await _upsertAlertsRequestHandler.UpsertAlertsAsync(alerts);
        }

        [HttpGet]
        public async Task<List<Alert>> GetAlerts(CancellationToken cancellationToken)
        {
            return await _getAlertsRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpPost("pushover")]
        public async Task UpsertPushover([FromBody] PushoverRequest request)
        {
            await _upsertPushoverRequestHandler.HandleAsync(request);
        }

        [HttpPost("pushover/test")]
        public async Task UpsertPushover(CancellationToken cancellationToken)
        {
            await _testPushoverClientRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpGet("pushover")]
        public async Task<PushoverRequest> GetPushover(CancellationToken cancellationToken)
        {
            return await _getPushoverClientRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpPost("webpush")]
        public async Task UpsertWebpush([FromBody] WebPushRequest request)
        {
            await _upsertWebPushRequestHandler.HandleAsync(request);
        }

        [HttpPost("webpush/test")]
        public async Task UpsertWebpush(CancellationToken cancellationToken)
        {
            await _testWebPushClientRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpGet("webpush")]
        public async Task<WebPushRequest> GetWebpush(CancellationToken cancellationToken)
        {
            return await _getWebPushClientRequestHandler.HandleAsync(cancellationToken);
        }
    }
}
