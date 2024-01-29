using System;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class ImageDownloadResponse
    {
        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("response_code")]
        public string ResponseCode { get; set; }

        [JsonPropertyName("transaction_id")]
        public Guid TransactionId { get; set; }
    }
}
