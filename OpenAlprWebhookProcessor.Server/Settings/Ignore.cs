using System;

namespace OpenAlprWebhookProcessor.Settings
{
    public class Ignore
    {
        public Guid Id { get; set; }

        public string PlateNumber { get; set; }

        public bool StrictMatch { get; set; }

        public string Description { get; set; }
    }
}
