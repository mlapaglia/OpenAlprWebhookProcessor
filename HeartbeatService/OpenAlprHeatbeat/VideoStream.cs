using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.HeartbeatService
{
    public class VideoStream
    {
        [JsonPropertyName("camera_id")]
        public int CameraId { get; set; }

        [JsonPropertyName("camera_name")]
        public string CameraName { get; set; }

        [JsonPropertyName("fps")]
        public int Fps { get; set; }

        [JsonPropertyName("is_streaming")]
        public bool IsStreaming { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("last_update")]
        public object LastUpdate { get; set; }

        [JsonPropertyName("last_plate_read")]
        public long LastPlateRead { get; set; }

        [JsonPropertyName("total_plate_reads")]
        public long TotalPlatesRead { get; set; }

        [JsonPropertyName("gps_latitude")]
        public double GpsLatitude { get; set; }

        [JsonPropertyName("gps_longitude")]
        public double GpsLongitude { get; set; }
    }
}
