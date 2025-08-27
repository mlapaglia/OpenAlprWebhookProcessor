using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket
{
    public class ImageDownloadRequestSize
    {
        public ImageDownloadRequestSize() { }

        [JsonPropertyName("type")]
        public string X { get; set; }

        [JsonPropertyName("uuid")]
        public string Y { get; set; }

        [JsonPropertyName("direction")]
        public int Width { get; set; }

        [JsonPropertyName("camera_id")]
        public int Height { get; set; }
    }
}