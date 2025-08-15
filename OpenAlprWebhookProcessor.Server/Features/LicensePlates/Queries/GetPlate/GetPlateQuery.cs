using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate
{
    public class GetPlateQuery : IQuery<LicensePlate?>
    {
        public Guid Id { get; set; }

        public GetPlateQuery(Guid id)
        {
            Id = id;
        }
    }
} 