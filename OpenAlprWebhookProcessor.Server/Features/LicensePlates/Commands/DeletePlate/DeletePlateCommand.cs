using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate
{
    public class DeletePlateCommand : ICommand
    {
        public Guid Id { get; set; }

        public DeletePlateCommand(Guid id)
        {
            Id = id;
        }
    }
} 