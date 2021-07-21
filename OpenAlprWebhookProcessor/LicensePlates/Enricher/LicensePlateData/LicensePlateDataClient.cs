using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.Enricher.LicensePlateData
{
    public class LicensePlateDataClient : ILicensePlateEnricherClient
    {
        private const string LicensePlateDataApiUrl = "https://licenseplatedata.com/consumer-api/$key/$state/$plate";

        private readonly HttpClient _httpClient;

        private readonly string _apiKey;

        public LicensePlateDataClient(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<EnrichedLicensePlate> GetLicenseInformationAsync(
            string plateNumber,
            string state,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(
                LicensePlateDataApiUrl
                    .Replace("$key", _apiKey)
                    .Replace("$state", state)
                    .Replace("$plate", plateNumber),
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                throw new ArgumentException("An error occurred while enriching with LicensePlateData API.");
            }

            var rawContent = await response.Content.ReadAsStreamAsync(cancellationToken);

            var parsed = await JsonSerializer.DeserializeAsync<LicensePlateDataRoot>(
                rawContent,
                cancellationToken: cancellationToken);


            if (parsed.Error)
            {
                throw new ArgumentException("An error occurred while enriching with LicensePlateData API: " + parsed.Message);
            }

            return new EnrichedLicensePlate()
            {
                Engine = parsed.LicensePlateLookup.Engine,
                Make = parsed.LicensePlateLookup.Make,
                Model = parsed.LicensePlateLookup.Model,
                Style = parsed.LicensePlateLookup.Style,
                Vin = parsed.LicensePlateLookup.Vin,
                Year = parsed.LicensePlateLookup.Year,
            };
        }
    }
}
