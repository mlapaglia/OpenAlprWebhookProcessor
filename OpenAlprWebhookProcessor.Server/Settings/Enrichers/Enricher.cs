using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using System;

namespace OpenAlprWebhookProcessor.Settings.Enrichers
{
    public class Enricher
    {
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public EnricherType EnricherType { get; set; }

        public string ApiKey { get; set; }

        public EnrichmentType EnrichmentType { get; set; }
    }
}
