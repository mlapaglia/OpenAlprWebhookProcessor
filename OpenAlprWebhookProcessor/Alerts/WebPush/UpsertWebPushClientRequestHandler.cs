using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.WebPush
{
    public class UpsertWebPushClientRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertWebPushClientRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(WebPushRequest request)
        {
            var webPushClient = await _processorContext.WebPushSettings.FirstOrDefaultAsync();

            webPushClient.IsEnabled = request.IsEnabled;
            webPushClient.Subject = request.EmailAddress;

            await _processorContext.SaveChangesAsync();
        }
    }
}
