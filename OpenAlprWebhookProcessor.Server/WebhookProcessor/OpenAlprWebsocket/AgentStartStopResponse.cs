using System;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class AgentStartStopResponse
    {
        [JsonPropertyName("type")]
        public string RequestType { get; set; }

        [JsonPropertyName("success")]
        public bool Success {  get; set; }

        [JsonPropertyName("version")]
        public string Version {  get; set; }

        [JsonPropertyName("transaction_id")]
        public Guid TransactionId { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("agent_epoch_ms")]
        public long AgentEpochMs { get; set; }

    }
}
