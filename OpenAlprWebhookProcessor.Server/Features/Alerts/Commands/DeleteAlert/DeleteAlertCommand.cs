using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.DeleteAlert
{
    public class DeleteAlertCommand : ICommand
    {
        public Guid Id { get; set; }

        public DeleteAlertCommand(Guid id)
        {
            Id = id;
        }
    }
}
