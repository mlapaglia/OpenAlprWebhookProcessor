using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Cameras.Configuration
{
    public class AgentConfiguration
    {
        public List<CameraConfiguration> Cameras { get; set; }

        public string Hostname { get; set; }

        public bool IgnoreSslErrors { get; set; }

        public Uri Endpoint { get; set; }

        public string Uid { get; set; }

        public string Version { get; set; }

        public string OpenAlprWebServerUrl { get; set; }

        public string OpenAlprWebServerApiKey { get; set; }
    }
}
