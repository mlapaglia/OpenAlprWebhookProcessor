using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Hydrator.OpenAlprSearch
{
    public class Response
    {
        [JsonPropertyName("pk")]
        public int Pk { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("fields")]
        public Fields Fields { get; set; }
    }
}
