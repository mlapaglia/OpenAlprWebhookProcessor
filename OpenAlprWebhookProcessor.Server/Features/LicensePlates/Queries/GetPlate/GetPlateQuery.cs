using MediatR;
using OpenAlprWebhookProcessor.Features;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate
{
    public class GetPlateQuery : IRequest<LicensePlate?>
    {
        public Guid Id { get; set; }

        public GetPlateQuery(Guid id)
        {
            Id = id;
        }
    }
} 