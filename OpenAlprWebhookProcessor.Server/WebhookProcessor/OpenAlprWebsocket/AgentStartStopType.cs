using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public enum AgentStartStopType
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        Start = 0,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        Stop = 1,
    }
}
