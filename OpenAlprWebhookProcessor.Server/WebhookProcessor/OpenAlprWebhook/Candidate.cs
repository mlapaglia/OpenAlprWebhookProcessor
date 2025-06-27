using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook
{
    public class Candidate
    {
        [JsonPropertyName("plate")]
        public string Plate { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("matches_template")]
        public int MatchesTemplate { get; set; }
    }
}
