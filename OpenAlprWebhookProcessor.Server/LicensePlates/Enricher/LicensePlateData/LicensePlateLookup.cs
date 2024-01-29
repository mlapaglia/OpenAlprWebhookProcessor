using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.LicensePlates.Enricher.LicensePlateData
{
    public class LicensePlateLookup
    {
        [JsonPropertyName("vin")]
        public string Vin { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("engine")]
        public string Engine { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("make")]
        public string Make { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }
    }
}
