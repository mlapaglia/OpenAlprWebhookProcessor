using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class GetPushoverClientRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetPushoverClientRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<PushoverRequest> HandleAsync(CancellationToken cancellationToken)
        {
            var client = await _processorContext.PushoverAlertClients.FirstOrDefaultAsync(cancellationToken);

            if (client == null)
            {
                return new PushoverRequest();
            }

            return new PushoverRequest()
            {
                ApiToken = client.ApiToken,
                IsEnabled = client.IsEnabled,
                SendPlatePreviewEnabled = client.SendPlatePreview,
                SendEveryPlateEnabled = client.SendEveryPlateEnabled,
                UserKey = client.UserKey,
            };
        }
    }
}
