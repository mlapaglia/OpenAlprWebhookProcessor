using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    [Authorize]
    [ApiController]
    [Route("alerts")]
    public class AlertsController : Controller
    {
        private readonly GetAlertsRequestHandler _getAlertsRequestHandler;

        private readonly UpsertAlertsRequestHandler _upsertAlertsRequestHandler;

        private readonly UpsertPushoverClientRequestHandler _upsertPushoverRequestHandler;

        private readonly GetPushoverClientRequestHandler _getPushoverClientRequestHandler;

        private readonly TestPushoverClientRequestHandler _testPushoverClientRequestHandler;

        public AlertsController(
            GetAlertsRequestHandler getAlertsRequestHandler,
            UpsertAlertsRequestHandler upsertAlertsRequestHandler,
            UpsertPushoverClientRequestHandler upsertPushoverRequestHandler,
            GetPushoverClientRequestHandler getPushoverClientRequestHandler,
            TestPushoverClientRequestHandler testPushoverClientRequestHandler)
        {
            _getAlertsRequestHandler = getAlertsRequestHandler;
            _upsertAlertsRequestHandler = upsertAlertsRequestHandler;
            _upsertPushoverRequestHandler = upsertPushoverRequestHandler;
            _getPushoverClientRequestHandler = getPushoverClientRequestHandler;
            _testPushoverClientRequestHandler = testPushoverClientRequestHandler;
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
    }
}
