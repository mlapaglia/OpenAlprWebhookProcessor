using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using System;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers
{
    public class EnricherDto
    {
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public EnricherType EnricherType { get; set; }

        public string ApiKey { get; set; }

        public EnrichmentType EnrichmentType { get; set; }
    }
} 