using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteIgnore
{
    public class DeleteIgnoreCommand : ICommand
    {
        public Guid Id { get; set; }

        public DeleteIgnoreCommand(Guid id)
        {
            Id = id;
        }
    }
}
