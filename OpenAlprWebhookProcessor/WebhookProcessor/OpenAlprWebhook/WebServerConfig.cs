using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebServerConfig
    {
        [JsonPropertyName("camera_label")]
        public string CameraLabel { get; set; }

        [JsonPropertyName("agent_label")]
        public string AgentLabel { get; set; }
    }
}
