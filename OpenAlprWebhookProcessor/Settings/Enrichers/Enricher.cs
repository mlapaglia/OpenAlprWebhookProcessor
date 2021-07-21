using OpenAlprWebhookProcessor.LicensePlates.Enricher;
using System;
using System.Text.Json.Serialization;

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
