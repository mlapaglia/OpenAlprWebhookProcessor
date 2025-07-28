using Mediator;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetAlerts
{
    public class GetAlertsQuery : IQuery<List<Alert>>
    {
        public GetAlertsQuery()
        {
        }
    }
} 