using Mediator;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpdateAlert
{
    public class UpdateAlertCommand : ICommand
    {
        public Alert Alert { get; set; }

        public UpdateAlertCommand(Alert alert)
        {
            Alert = alert;
        }
    }
}
