using System;

namespace OpenAlprWebhookProcessor.Settings
{
    public class Agent
    {
        public string EndpointUrl { get; set; }

        public Guid Id { get; set; }

        public string Hostname { get; set; }

        public string Uid { get; set; }

        public string OpenAlprWebServerUrl { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public int SunriseOffset { get; set; }

        public int SunsetOffset { get; set; }

        public double TimezoneOffset { get; set; }

        public bool IsDebugEnabled { get; set; }

        public bool IsImageCompressionEnabled { get; set; }

        public long LastHeartbeatEpochMs { get; set; }

        public int? ScheduledScrapingIntervalMinutes { get; set; }

        public int? NextScrapeInMinutes { get; set; }
    }
}