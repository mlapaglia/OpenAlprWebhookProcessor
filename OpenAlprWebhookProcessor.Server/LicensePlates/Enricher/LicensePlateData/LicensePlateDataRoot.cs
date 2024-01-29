using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.LicensePlates.Enricher.LicensePlateData
{
    public class LicensePlateDataRoot
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("query_time")]
        public string QueryTime { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("requestIP")]
        public string RequestIp { get; set; }

        [JsonPropertyName("licensePlateLookup")]
        public LicensePlateLookup LicensePlateLookup { get; set; }

        [JsonPropertyName("cache")]
        public bool Cache { get; set; }
    }
}
