using System.Collections.Generic;
using System.Threading;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public interface IAlertService
    {
        void AddJob(AlertUpdateRequest request);

        int GetPendingAlertsCount();

        IEnumerable<AlertUpdateRequest> GetConsumingAlerts(CancellationToken cancellationToken);
    }
}