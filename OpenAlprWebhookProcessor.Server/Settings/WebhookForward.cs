using System;

namespace OpenAlprWebhookProcessor.Settings
{
    public class WebhookForward
    {
        public Guid Id { get; set; }

        public Uri? Destination { get; set; }

        public bool IgnoreSslErrors { get; set; }

        public bool ForwardGroupPreviews { get; set; }

        public bool ForwardSinglePlates { get; set; }

        public bool ForwardGroups { get; set; }
    }
}
