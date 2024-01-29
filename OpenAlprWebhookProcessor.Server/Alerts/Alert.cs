using System;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class Alert
    {
        public Guid Id { get; set; }

        public bool IsUrgent { get; set; }

        public string PlateNumber { get; set; }

        public bool StrictMatch { get; set; }

        public string Description { get; set; }
    }
}
