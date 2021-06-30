using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class AlertsController : Controller
    {
        private readonly GetAlertsRequestHandler _getAlertsRequestHandler;

        private readonly UpsertAlertsRequestHandler _upsertAlertsRequestHandler;

        private readonly UpsertPushoverRequestHandler _upsertPushoverRequestHandler;
        public AlertsController(
            GetAlertsRequestHandler getAlertsRequestHandler,
            UpsertAlertsRequestHandler upsertAlertsRequestHandler,
            UpsertPushoverRequestHandler upsertPushoverRequestHandler)
        {
            _getAlertsRequestHandler = getAlertsRequestHandler;
            _upsertAlertsRequestHandler = upsertAlertsRequestHandler;
            _upsertPushoverRequestHandler = upsertPushoverRequestHandler;
        }

        [HttpPost("alerts/add")]
        public async Task AddAlert([FromBody] Alert alert)
        {
            await _upsertAlertsRequestHandler.AddAlertAsync(alert);
        }

        [HttpPost("alerts")]
        public async Task UpsertAlerts([FromBody] List<Alert> alerts)
        {
            await _upsertAlertsRequestHandler.UpsertAlertsAsync(alerts);
        }

        [HttpGet("alerts")]
        public async Task<List<Alert>> GetAlerts(CancellationToken cancellationToken)
        {
            return await _getAlertsRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpPost("pushover")]
        public async Task UpsertPushover([FromBody] UpsertPushoverRequest request)
        {
            await _upsertPushoverRequestHandler.HandleAsync(request);
        }
    }
}
