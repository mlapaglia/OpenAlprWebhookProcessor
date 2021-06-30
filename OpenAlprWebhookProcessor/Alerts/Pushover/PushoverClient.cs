using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class PushoverClient : IAlertClient
    {
        private const string PushOverApiUrl = "https://api.pushover.net/1/messages.json";

        private readonly HttpClient _httpClient;

        private readonly PushoverConfiguration _pushoverConfiguration;

        public PushoverClient(PushoverConfiguration pushoverConfiguration)
        {
            _pushoverConfiguration = pushoverConfiguration;
            _httpClient = new HttpClient();
        }
        public async Task SendAlertAsync(Alert alert)
        {
            var pushUrl = PushOverApiUrl + $"?token={_pushoverConfiguration.AppToken}&user={_pushoverConfiguration.UserKey}&message={alert.PlateNumber}";

            await _httpClient.PostAsync(pushUrl, null);
        }
    }
}
