using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate
{
    public class DeletePlateCommand : IRequest
    {
        public Guid Id { get; set; }

        public DeletePlateCommand(Guid id)
        {
            Id = id;
        }
    }
} 