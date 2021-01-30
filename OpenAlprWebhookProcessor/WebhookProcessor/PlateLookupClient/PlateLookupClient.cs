using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.PlateLookupClient
{
    public static class PlateLookupClient
    {
        public static async Task<Response> LookupPlateAsync(
            string plateNumber,
            string state,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient();

            var result = await client.GetAsync(
                $"https://www.autocheck.com/consumer-api/meta/v1/summary/plate/{plateNumber}/state/{state}",
                cancellationToken);

            var response = await result.Content.ReadAsStringAsync(cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException("plate number and state not found.");
            }

            var parsedPlate = JsonSerializer.Deserialize<List<Response>>(response).FirstOrDefault();

            if (parsedPlate == null)
            {
                throw new ArgumentException("invalid plate response.");
            }

            return parsedPlate;
        }
    }
}
