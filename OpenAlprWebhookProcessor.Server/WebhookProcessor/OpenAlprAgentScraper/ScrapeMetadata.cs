using System.Text.Json.Serialization;

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
