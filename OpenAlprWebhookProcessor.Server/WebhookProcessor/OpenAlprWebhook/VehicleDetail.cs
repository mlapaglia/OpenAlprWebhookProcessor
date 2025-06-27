using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook
{
    public class VehicleDetail
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
