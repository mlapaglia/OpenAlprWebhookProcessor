using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate
{
    public class EnrichPlateCommand : IRequest
    {
        public Guid PlateId { get; set; }

        public EnrichPlateCommand(Guid plateId)
        {
            PlateId = plateId;
        }
    }
} 