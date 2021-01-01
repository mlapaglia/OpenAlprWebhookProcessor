using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class Plate
    {
        [JsonPropertyName("plate")]
        public string PlateNumber { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("matches_template")]
        public int MatchesTemplate { get; set; }

        [JsonPropertyName("plate_index")]
        public int PlateIndex { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("region_confidence")]
        public double RegionConfidence { get; set; }

        [JsonPropertyName("processing_time_ms")]
        public double ProcessingTimeMs { get; set; }

        [JsonPropertyName("requested_topn")]
        public int RequestedTopN { get; set; }

        [JsonPropertyName("coordinates")]
        public List<Coordinate> Coordinates { get; set; }

        [JsonPropertyName("plate_crop_jpeg")]
        public string PlateCropJpeg { get; set; }

        [JsonPropertyName("vehicle_region")]
        public VehicleRegion VehicleRegion { get; set; }

        [JsonPropertyName("vehicle_detected")]
        public bool VehicleDetected { get; set; }

        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }
}
