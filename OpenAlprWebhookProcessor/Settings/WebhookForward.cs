using System;

namespace OpenAlprWebhookProcessor.Settings
{
    public class WebhookForward
    {
        public Guid Id { get; set; }

        public Uri? Destination { get; set; }

        public bool IgnoreSslErrors { get; set; }
    }
}
