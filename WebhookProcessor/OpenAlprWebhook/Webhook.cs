
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook
{
    public class Webhook
    {
        [JsonPropertyName("data_type")]
        public string DataType { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("epoch_time")]
        public long EpochTime { get; set; }

        [JsonPropertyName("agent_uid")]
        public string AgentUid { get; set; }

        [JsonPropertyName("alert_list")]
        public string AlertList { get; set; }

        [JsonPropertyName("site_name")]
        public string SiteName { get; set; }

        [JsonPropertyName("camera_name")]
        public string CameraName { get; set; }

        [JsonPropertyName("camera_number")]
        public int CameraNumber { get; set; }

        [JsonPropertyName("plate_number")]
        public string PlateNumber { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("list_type")]
        public string ListType { get; set; }

        [JsonPropertyName("group")]
        public Group Group { get; set; }
    }
}
