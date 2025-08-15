using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate
{
    public class EnrichPlateCommand : ICommand
    {
        public Guid PlateId { get; set; }

        public EnrichPlateCommand(Guid plateId)
        {
            PlateId = plateId;
        }
    }
} 