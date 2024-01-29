using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper
{
    public class ScrapeMetadata
    {
        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }
    }
}
