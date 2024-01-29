using System;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class ConfigSaveMaskRequest
    {
        [JsonPropertyName("type")]
        public string RequestType { get; set; }

        [JsonPropertyName("transaction_id")]
        public Guid TransactionId { get; set; }

        [JsonPropertyName("stream_file")]
        public string StreamFile { get; set; }

        [JsonPropertyName("mask_image")]
        public string MaskImage { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }
    }
}
