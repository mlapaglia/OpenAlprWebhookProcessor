using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class Year
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
