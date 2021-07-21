using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.Enricher.LicensePlateData
{
    public class LicensePlateDataClient : ILicensePlateEnricherClient
    {
        private readonly ILogger _logger;

        private const string LicensePlateDataApiUrl = "https://licenseplatedata.com/consumer-api/$key/$state/$plate";

        private const string TestPlateNumber = "TEST";

        private const string TestPlateState = "XX";

        private readonly HttpClient _httpClient;

        private readonly ProcessorContext _processorContext;

        public LicensePlateDataClient(
            ProcessorContext processorContext,
            ILogger<LicensePlateDataClient> logger)
        {
            _httpClient = new HttpClient();
            _processorContext = processorContext;
            _logger = logger;
        }

        public async Task<EnrichedLicensePlate> GetLicenseInformationAsync(
            string plateNumber,
            string state,
            CancellationToken cancellationToken)
        {
            

            var response = await _httpClient.GetAsync(
                LicensePlateDataApiUrl
                    .Replace("$key", await GetApiKeyAsync(cancellationToken))
                    .Replace("$state", state)
                    .Replace("$plate", plateNumber),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = "An error occurred while enriching with LicensePlateData API: " + await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(message);
                throw new ArgumentException(message);
            }

            var rawContent = await response.Content.ReadAsStreamAsync(cancellationToken);

            var parsed = await JsonSerializer.DeserializeAsync<LicensePlateDataRoot>(
                rawContent,
                cancellationToken: cancellationToken);

            if (parsed.Error)
            {
                var errorMessage = "An error occurred while enriching with LicensePlateData API: " + parsed.Message;
                _logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
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

        public async Task<bool> TestAsync(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(
                LicensePlateDataApiUrl
                    .Replace("$key", await GetApiKeyAsync(cancellationToken))
                    .Replace("$state", TestPlateState)
                    .Replace("$plate", TestPlateNumber),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("An error occurred while enriching with LicensePlateData API: " + await response.Content.ReadAsStringAsync(cancellationToken));
                return false;
            }

            var parsed = await JsonSerializer.DeserializeAsync<LicensePlateDataRoot>(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);

            if (parsed.Error)
            {
                _logger.LogError("An error occurred while testing: " + parsed.Message);
                return false;
            }

            return true;
        }

        private async Task<string> GetApiKeyAsync(CancellationToken cancellationToken)
        {
            var enricher = await _processorContext.Enrichers.FirstOrDefaultAsync(cancellationToken);
            return enricher.ApiKey;
        }
    }
}
