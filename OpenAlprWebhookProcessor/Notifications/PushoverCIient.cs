using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Notifications
{
    public interface INotificationClient
    {

    }

    public class PushoverCIient : INotificationClient
    {
        private readonly HttpClient _httpClient;

        public PushoverCIient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> SendNotificationAsync(User user)
        {
            return true;
        }
    }
}
