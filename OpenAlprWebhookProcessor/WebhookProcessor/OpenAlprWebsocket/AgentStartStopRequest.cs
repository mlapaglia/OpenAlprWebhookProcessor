using System;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class AgentStartStopRequest
    {
        [JsonPropertyName("type")]
        public string RequestType { get; set; }

        [JsonPropertyName("agent_id")]
        public string AgentId {  get; set; }

        [JsonPropertyName("agent_op")]
        public string AgentOp {  get; set; }

        [JsonPropertyName("transaction_id")]
        public Guid TransactionId { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }
    }
}
