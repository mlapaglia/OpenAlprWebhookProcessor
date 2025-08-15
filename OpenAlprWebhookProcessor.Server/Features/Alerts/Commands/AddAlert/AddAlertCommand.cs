using Mediator;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert
{
    public class AddAlertCommand : ICommand
    {
        public Alert Alert { get; set; }

        public AddAlertCommand(Alert alert)
        {
            Alert = alert;
        }
    }
} 