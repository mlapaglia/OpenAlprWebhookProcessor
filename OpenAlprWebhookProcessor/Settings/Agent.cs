namespace OpenAlprWebhookProcessor.Settings
{
    public class Agent
    {
        public string EndpointUrl { get; set; }

        public string Hostname { get; set; }

        public string Uid { get; set; }

        public string OpenAlprWebServerApiKey { get; set; }

        public string OpenAlprWebServerUrl { get; set; }

        public string Version { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int SunriseOffset { get; set; }

        public int SunsetOffset { get; set; }

        public double TimezoneOffset { get; set; }

        public bool IsDebugEnabled { get; set; }
    }
}
