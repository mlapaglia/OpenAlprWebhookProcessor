using OpenAlprWebhookProcessor.Server.LicensePlates.Enricher;
using OpenAlprWebhookProcessor.Server.Settings.Enrichers;
using System;

namespace OpenAlprWebhookProcessor.Server.Data
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
