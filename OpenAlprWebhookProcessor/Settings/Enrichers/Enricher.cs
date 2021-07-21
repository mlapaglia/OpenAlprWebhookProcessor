using OpenAlprWebhookProcessor.LicensePlates.Enricher;
using System;

namespace OpenAlprWebhookProcessor.Settings.Enrichers
{
    public class Enricher
    {
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public EnricherType EnricherType { get; set; }

        public string ApiKey { get; set; }

        public bool RunAlways { get; set; }

        public bool RunAtNight { get; set; }

        public bool RunManually { get; set; }
    }
}
