using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class Vehicle
    {
        [JsonPropertyName("color")]
        public List<VehicleDetail> Colors { get; set; }

        [JsonPropertyName("make")]
        public List<VehicleDetail> Makes { get; set; }

        [JsonPropertyName("make_model")]
        public List<VehicleDetail> MakeModels { get; set; }

        [JsonPropertyName("body_type")]
        public List<VehicleDetail> BodyTypes { get; set; }

        [JsonPropertyName("year")]
        public List<VehicleDetail> Years { get; set; }

        [JsonPropertyName("orientation")]
        public List<VehicleDetail> Orientations { get; set; }

        [JsonPropertyName("missing_plate")]
        public List<VehicleDetail> MissingPlates { get; set; }

        [JsonPropertyName("is_vehicle")]
        public List<VehicleDetail> IsVehicles { get; set; }

    }
}
