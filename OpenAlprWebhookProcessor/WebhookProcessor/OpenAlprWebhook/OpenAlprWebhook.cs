using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class OpenAlprWebhook
    {
        [JsonPropertyName("data_type")]
        public string DataType { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("epoch_start")]
        public long EpochStart { get; set; }

        [JsonPropertyName("epoch_end")]
        public long EpochEnd { get; set; }

        [JsonPropertyName("frame_start")]
        public int FrameStart { get; set; }

        [JsonPropertyName("frame_end")]
        public int FrameEnd { get; set; }

        [JsonPropertyName("company_id")]
        public string CompanId { get; set; }

        [JsonPropertyName("agent_uid")]
        public string AgentUid { get; set; }

        [JsonPropertyName("agent_version")]
        public string AgentVersion { get; set; }

        [JsonPropertyName("agent_type")]
        public string AgenType { get; set; }

        [JsonPropertyName("camera_id")]
        public int CameraId { get; set; }

        [JsonPropertyName("gps_latitude")]
        public double GpsLatitude { get; set; }

        [JsonPropertyName("gps_longitude")]
        public double GpsLongitude { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("uuids")]
        public List<string> Uuids { get; set; }

        [JsonPropertyName("plate_indexes")]
        public List<int> PlateIndexes { get; set; }

        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }

        [JsonPropertyName("vehicle_crop_jpeg")]
        public string VehicleCropJpeg { get; set; }

        [JsonPropertyName("best_plate")]
        public Plate BestPlate { get; set; }

        [JsonPropertyName("best_confidence")]
        public double Plate { get; set; }

        [JsonPropertyName("best_uuid")]
        public string BestUuid { get; set; }

        [JsonPropertyName("best_plate_number")]
        public string BestPlateNumber { get; set; }

        [JsonPropertyName("best_region")]
        public string BestRegion { get; set; }

        [JsonPropertyName("best_region_confidence")]
        public int BestRegionConfidence { get; set; }

        [JsonPropertyName("matches_template")]
        public bool MatchesTemplate { get; set; }

        [JsonPropertyName("best_image_width")]
        public int BestImageWidth { get; set; }

        [JsonPropertyName("best_image_height")]
        public int BeightImageHeight { get; set; }

        [JsonPropertyName("travel_direction")]
        public double TravelDirection { get; set; }

        [JsonPropertyName("is_parked")]
        public bool IsParked { get; set; }

        [JsonPropertyName("is_preview")]
        public bool IsPreview { get; set; }

        [JsonPropertyName("vehicle")]
        public Vehicle Vehicle { get; set; }

        [JsonPropertyName("web_server_config")]
        public WebServerConfig WebServerConfig { get; set; }

        [JsonPropertyName("direction_of_travel_id")]
        public int DirectionOfTravelId { get; set; }
    }
}
