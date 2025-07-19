using MediatR;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert
{
    public class AddAlertCommand : IRequest
    {
        public Alert Alert { get; set; }

        public AddAlertCommand(Alert alert)
        {
            Alert = alert;
        }
    }
} 