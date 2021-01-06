using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Cameras.Configuration
{
    public class AgentConfiguration
    {
        public List<CameraConfiguration> Cameras { get; set; }

        public string Hostname { get; set; }

        public OpenAlprServerConfiguration OpenAlprWebServer { get; set; }

        public string Uid { get; set; }

        public string Version { get; set; }
    }
}
