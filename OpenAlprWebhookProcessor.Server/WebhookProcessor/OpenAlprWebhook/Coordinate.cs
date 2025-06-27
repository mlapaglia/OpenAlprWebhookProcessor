using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook
{
    public class Coordinate
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }
}
