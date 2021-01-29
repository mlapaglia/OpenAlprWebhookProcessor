using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class Agent
    {
        public Guid Id { get; set; }

        public string EndpointUrl { get; set; }

        public string Hostname { get; set; }

        public string Uid { get; set; }

        public string OpenAlprWebServerApiKey { get; set; }

        public string OpenAlprWebServerUrl { get; set; }

        public string Version { get; set; }
    }
}
