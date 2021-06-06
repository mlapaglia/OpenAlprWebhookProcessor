using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class WebhookForward
    {
        public Guid Id { get; set; }

        public Uri FowardingDestination { get; set; }

        public bool IgnoreSslErrors { get; set; }

        public bool ForwardSinglePlates { get; set; }

        public bool ForwardGroupPreviews { get; set; }

        public bool ForwardGroups { get; set; }
    }
}
