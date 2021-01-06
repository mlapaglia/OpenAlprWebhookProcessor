using System;

namespace OpenAlprWebhookProcessor.Cameras.Configuration
{
    public class OpenAlprServerConfiguration
    {
        public bool IgnoreSslErrors { get; set; }

        public Uri Endpoint { get; set; }
    }
}
