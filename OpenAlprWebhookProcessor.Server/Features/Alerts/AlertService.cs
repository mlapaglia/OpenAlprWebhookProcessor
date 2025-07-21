using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public class AlertService : IAlertService
    {
        private readonly BlockingCollection<AlertUpdateRequest> _alertsToProcess = new BlockingCollection<AlertUpdateRequest>();

        public void AddJob(AlertUpdateRequest request)
        {
            _alertsToProcess.Add(request);
        }

        public int GetPendingAlertsCount()
        {
            return _alertsToProcess.Count;
        }

        public IEnumerable<AlertUpdateRequest> GetConsumingAlerts(CancellationToken cancellationToken)
        {
            return _alertsToProcess.GetConsumingEnumerable(cancellationToken);
        }
    }
}