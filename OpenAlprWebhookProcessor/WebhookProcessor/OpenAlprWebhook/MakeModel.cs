using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class MakeModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
