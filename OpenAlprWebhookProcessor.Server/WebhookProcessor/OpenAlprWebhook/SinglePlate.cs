using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook
{
    public class SinglePlate
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("data_type")]
        public string DataType { get; set; }

        [JsonPropertyName("epoch_time")]
        public long EpochTime { get; set; }

        [JsonPropertyName("img_width")]
        public int ImgWidth { get; set; }

        [JsonPropertyName("img_height")]
        public int ImgHeight { get; set; }

        [JsonPropertyName("processing_time_ms")]
        public double ProcessingTimeMs { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("regions_of_interest")]
        public List<RegionsOfInterest> RegionsOfInterest { get; set; }

        [JsonPropertyName("vehicles")]
        public List<object> Vehicles { get; set; }

        [JsonPropertyName("results")]
        public List<Result> Results { get; set; }

        [JsonPropertyName("camera_id")]
        public int CameraId { get; set; }

        [JsonPropertyName("agent_uid")]
        public string AgentUid { get; set; }

        [JsonPropertyName("agent_version")]
        public string AgentVersion { get; set; }

        [JsonPropertyName("agent_type")]
        public string AgentType { get; set; }

        [JsonPropertyName("company_id")]
        public string CompanyId { get; set; }
    }
}
