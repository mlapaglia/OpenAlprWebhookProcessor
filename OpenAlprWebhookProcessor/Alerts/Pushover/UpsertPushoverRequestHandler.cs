using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class UpsertPushoverRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertPushoverRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(UpsertPushoverRequest request)
        {
            var pushoverClient = await _processorContext.PushoverAlertClients.FirstOrDefaultAsync();

            bool isAdding = false;

            if (pushoverClient == null)
            {
                isAdding = true;

                pushoverClient = new Data.Pushover()
                {
                    ApiToken = request.ApiToken,
                    IsEnabled = request.IsEnabled,
                    SendPlatePreview = request.SendPlatePreview,
                    UserKey = request.UserKey,
                };
            }
            else
            {
                pushoverClient.ApiToken = request.ApiToken;
                pushoverClient.IsEnabled = request.IsEnabled;
                pushoverClient.SendPlatePreview = request.SendPlatePreview;
                pushoverClient.UserKey = request.UserKey;
            }

            if (isAdding)
            {
                _processorContext.PushoverAlertClients.Add(pushoverClient);
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
