using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook
{
    public class WebServerConfig
    {
        [JsonPropertyName("camera_label")]
        public string CameraLabel { get; set; }

        [JsonPropertyName("agent_label")]
        public string AgentLabel { get; set; }
    }
}
