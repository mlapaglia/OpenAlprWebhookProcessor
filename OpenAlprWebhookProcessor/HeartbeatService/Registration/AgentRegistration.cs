using OpenAlprWebhookProcessor.HeartbeatService.Registration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.HeartbeatService
{
    public static class AgentRegistration
    {
        public static async Task<string> RegisterAgentAsync(
            Uri serverUrl,
            string username,
            string password,
            bool ignoreSslErrors)
        {
            var clientHandler = new HttpClientHandler();

            if (ignoreSslErrors)
            {
                clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            }

            var httpClient = new HttpClient(clientHandler);

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var response = await httpClient.PostAsync(
                $"{serverUrl}api/accountinfo",
                formContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var agentRegistrationResponse = JsonSerializer.Deserialize<RegistrationResponse>(responseString);

            return agentRegistrationResponse.CompanyId;
        }
    }
}
