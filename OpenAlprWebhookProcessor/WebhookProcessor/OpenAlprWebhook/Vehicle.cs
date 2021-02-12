using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class Vehicle
    {
        [JsonPropertyName("color")]
        public List<Color> Color { get; set; }

        [JsonPropertyName("make")]
        public List<Make> Make { get; set; }

        [JsonPropertyName("make_model")]
        public List<MakeModel> MakeModel { get; set; }

        [JsonPropertyName("body_type")]
        public List<BodyType> BodyType { get; set; }

        [JsonPropertyName("year")]
        public List<Year> Year { get; set; }

        [JsonPropertyName("orientation")]
        public List<Orientation> Orientation { get; set; }
    }
}
