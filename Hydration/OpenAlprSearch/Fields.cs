using System.Text.Json.Serialization;
using static OpenAlprWebhookProcessor.Hydration.StringToIntConverter;

namespace OpenAlprWebhookProcessor.Hydrator.OpenAlprSearch
{
    public class Fields
    {
        [JsonPropertyName("agent_uid")]
        public string AgentUid { get; set; }

        [JsonPropertyName("best_confidence")]
        public string BestConfidence { get; set; }

        [JsonPropertyName("best_plate")]
        public string BestPlate { get; set; }

        [JsonPropertyName("best_index")]
        public int BestIndex { get; set; }

        [JsonPropertyName("best_uuid")]
        public string BestUuid { get; set; }

        [JsonConverter(typeof(IntToStringConverter))]
        [JsonPropertyName("camera")]
        public string Camera { get; set; }

        [JsonPropertyName("camera_id")]
        public int CameraId { get; set; }

        [JsonPropertyName("company")]
        public int Company { get; set; }

        [JsonPropertyName("crop_location")]
        public int CropLocation { get; set; }

        [JsonPropertyName("direction_of_travel_degrees")]
        public double DirectionOfTravelDegrees { get; set; }

        [JsonPropertyName("direction_of_travel_id")]
        public int DirectionOfTravelId { get; set; }

        [JsonPropertyName("epoch_time_start")]
        public string EpochTimeStart { get; set; }

        [JsonPropertyName("epoch_time_end")]
        public string EpochTimeEnd { get; set; }

        [JsonPropertyName("hit_count")]
        public int HitCount { get; set; }

        [JsonPropertyName("img_width")]
        public int ImgWidth { get; set; }

        [JsonPropertyName("img_height")]
        public int ImgHeight { get; set; }

        [JsonPropertyName("plate_x1")]
        public int PlateX1 { get; set; }

        [JsonPropertyName("plate_y1")]
        public int PlateY1 { get; set; }

        [JsonPropertyName("plate_x2")]
        public int PlateX2 { get; set; }

        [JsonPropertyName("plate_y2")]
        public int PlateY2 { get; set; }

        [JsonPropertyName("plate_x3")]
        public int PlateX3 { get; set; }

        [JsonPropertyName("plate_y3")]
        public int PlateY3 { get; set; }

        [JsonPropertyName("plate_x4")]
        public int PlateX4 { get; set; }

        [JsonPropertyName("plate_y4")]
        public int PlateY4 { get; set; }

        [JsonPropertyName("processing_time_ms")]
        public string ProcessingTimeMs { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("region_confidence")]
        public string RegionConfidence { get; set; }

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("site_id")]
        public int SiteId { get; set; }

        [JsonPropertyName("vehicle_body_type")]
        public string VehicleBodyType { get; set; }

        [JsonPropertyName("vehicle_color")]
        public string VehicleColor { get; set; }

        [JsonPropertyName("vehicle_make")]
        public string VehicleMake { get; set; }

        [JsonPropertyName("vehicle_make_model")]
        public string VehicleMakeModel { get; set; }

        [JsonPropertyName("vehicle_color_confidence")]
        public string VehicleColorConfidence { get; set; }

        [JsonPropertyName("vehicle_make_confidence")]
        public string VehicleMakeConfidence { get; set; }

        [JsonPropertyName("vehicle_make_model_confidence")]
        public string VehicleMakeModelConfidence { get; set; }

        [JsonPropertyName("vehicle_body_type_confidence")]
        public string VehicleBodyTypeConfidence { get; set; }

        [JsonPropertyName("vehicle_region_x")]
        public int VehicleRegionX { get; set; }

        [JsonPropertyName("vehicle_region_y")]
        public int VehicleRegionY { get; set; }

        [JsonPropertyName("vehicle_region_height")]
        public int VehicleRegionHeight { get; set; }

        [JsonPropertyName("vehicle_region_width")]
        public int VehicleRegionWidth { get; set; }
    }
}
