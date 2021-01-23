using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class Ignore
    {
        public Guid Id { get; set; }

        public string PlateNumber { get; set; }

        public bool IsStrictMatch { get; set; }

        public string Description { get; set; }
    }
}
