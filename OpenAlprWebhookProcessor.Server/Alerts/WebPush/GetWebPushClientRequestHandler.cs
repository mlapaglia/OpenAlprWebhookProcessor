using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;

namespace OpenAlprWebhookProcessor.Alerts.WebPush
{
    public class GetWebPushClientRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetWebPushClientRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<WebPushRequest> HandleAsync(CancellationToken cancellationToken)
        {
            var client = await _processorContext.WebPushSettings.FirstOrDefaultAsync(cancellationToken);

            if (client == null)
            {
                client = VapidKeyHelper.AddVapidKeys(_processorContext);
            }

            return new WebPushRequest()
            {
                IsEnabled = client.IsEnabled,
                EmailAddress = client.Subject,
                PublicKey = client.PublicKey,
                PrivateKey = client.PrivateKey,
                SendEveryPlateEnabled = client.SendEveryPlateEnabled,
            };
        }
    }
}
