using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class AccountInfoResponse
    {
        [JsonPropertyName("company_id")]
        public string CompanyId { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("creation_date")]
        public string CreationDate { get; set; }

        [JsonPropertyName("company_size")]
        public string CompanySize { get; set; }

        [JsonPropertyName("company_type")]
        public string CompanyType { get; set; }

        [JsonPropertyName("company_type_other")]
        public object CompanyTypeOther { get; set; }

        [JsonPropertyName("company_country")]
        public object CompanyCountry { get; set; }

        [JsonPropertyName("first_name")]
        public string Firstname { get; set; }

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
