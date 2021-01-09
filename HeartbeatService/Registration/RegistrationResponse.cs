using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.HeartbeatService.Registration
{
    public class RegistrationResponse
    {
        [JsonPropertyName("company_id")]
        public string CompanyId { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("creation_date")]
        public string CreationDate { get; set; }

        [JsonPropertyName("company_size")]
        public object CompanySize { get; set; }

        [JsonPropertyName("company_type")]
        public object CompanyType { get; set; }

        [JsonPropertyName("company_type_other")]
        public object CompanyTypeOther { get; set; }

        [JsonPropertyName("company_country")]
        public object CompanyCountry { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("websockets_url")]
        public string WebsocketsUrl { get; set; }
    }
}
