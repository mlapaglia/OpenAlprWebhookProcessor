using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class UpsertPushoverClientRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertPushoverClientRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(PushoverRequest request)
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
                    SendPlatePreview = request.SendPlatePreviewEnabled,
                    SendEveryPlateEnabled = request.SendEveryPlateEnabled,
                    UserKey = request.UserKey,
                };
            }
            else
            {
                pushoverClient.ApiToken = request.ApiToken;
                pushoverClient.IsEnabled = request.IsEnabled;
                pushoverClient.SendPlatePreview = request.SendPlatePreviewEnabled;
                pushoverClient.SendEveryPlateEnabled = request.SendEveryPlateEnabled;
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
