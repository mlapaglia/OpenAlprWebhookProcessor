using Mediator;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertAlerts
{
    public class UpsertAlertsCommand : ICommand
    {
        public List<Alert> Alerts { get; set; }

        public UpsertAlertsCommand(List<Alert> alerts)
        {
            Alerts = alerts;
        }
    }
} 