using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookRelayConfiguration
    {
        public bool IgnoreSslErrors { get; set; }

        public List<Uri> RelayUrls { get; set; }
    }
}
