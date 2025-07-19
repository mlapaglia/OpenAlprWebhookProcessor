using MediatR;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetAlerts
{
    public class GetAlertsQuery : IRequest<List<Alert>>
    {
        public GetAlertsQuery()
        {
        }
    }
} 