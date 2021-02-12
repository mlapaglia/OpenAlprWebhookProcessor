using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.PlateLookupClient
{
    public class Response
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("vin")]
        public string Vin { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("make")]
        public string Make { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("countryOfAssembly")]
        public string CountryOfAssembly { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("vehicleClass")]
        public string VehicleClass { get; set; }

        [JsonPropertyName("recordCount")]
        public int RecordCount { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("scoreRangeLow")]
        public int ScoreRangeLow { get; set; }

        [JsonPropertyName("scoreRangeHigh")]
        public int ScoreRangeHigh { get; set; }

        [JsonPropertyName("buybackAssurance")]
        public string BuybackAssurance { get; set; }

        [JsonPropertyName("lemonRecord")]
        public bool LemonRecord { get; set; }

        [JsonPropertyName("accidentRecord")]
        public bool AccidentRecord { get; set; }

        [JsonPropertyName("floodRecord")]
        public bool FloodRecord { get; set; }

        [JsonPropertyName("singleOwner")]
        public bool SingleOwner { get; set; }

        [JsonPropertyName("engine")]
        public string Engine { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
