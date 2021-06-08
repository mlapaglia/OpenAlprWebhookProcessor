using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class Vehicle
    {
        [JsonPropertyName("color")]
        public List<VehicleDetail> Color { get; set; }

        [JsonPropertyName("make")]
        public List<VehicleDetail> Make { get; set; }

        [JsonPropertyName("make_model")]
        public List<VehicleDetail> MakeModel { get; set; }

        [JsonPropertyName("body_type")]
        public List<VehicleDetail> BodyType { get; set; }

        [JsonPropertyName("year")]
        public List<VehicleDetail> Year { get; set; }

        [JsonPropertyName("orientation")]
        public List<VehicleDetail> Orientation { get; set; }

        [JsonPropertyName("missing_plate")]
        public List<VehicleDetail> MissingPlate { get; set; }

        [JsonPropertyName("is_vehicle")]
        public List<VehicleDetail> IsVehicle { get; set; }

    }
}
