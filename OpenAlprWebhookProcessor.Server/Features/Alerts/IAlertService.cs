using System.Collections.Generic;
using System.Threading;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public interface IAlertService
    {
        void AddJob(AlertUpdateRequest request);

        int GetPendingAlertsCount();

        IAsyncEnumerable<AlertUpdateRequest> GetConsumingAlertsAsync(CancellationToken cancellationToken = default);
    }
}