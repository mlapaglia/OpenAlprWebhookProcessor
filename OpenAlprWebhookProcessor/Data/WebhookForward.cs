using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class WebhookForward
    {
        public Guid Id { get; set; }

        public Uri FowardingDestination { get; set; }

        public bool IgnoreSslErrors { get; set; }
    }
}
