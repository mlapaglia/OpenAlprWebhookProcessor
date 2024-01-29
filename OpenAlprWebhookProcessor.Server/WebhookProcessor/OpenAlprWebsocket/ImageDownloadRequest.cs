using System;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class ImageDownloadRequest
    {
        public ImageDownloadRequest() { }


        [JsonPropertyName("type")]
        public string RequestType { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("camera_id")]
        public long CameraId { get; set; }

        [JsonPropertyName("transaction_id")]
        public Guid TransactionId { get; set; }
    }
}