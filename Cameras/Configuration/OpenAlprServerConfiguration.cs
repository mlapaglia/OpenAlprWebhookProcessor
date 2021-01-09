using System;

namespace OpenAlprWebhookProcessor.Cameras.Configuration
{
    public class OpenAlprServerConfiguration
    {
        public bool IgnoreSslErrors { get; set; }

        public Uri Endpoint { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
