using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions
{
    public class PushServiceClientWrapper : IPushServiceClientWrapper
    {
        private readonly PushServiceClient _pushServiceClient;

        public PushServiceClientWrapper(PushServiceClient pushServiceClient)
        {
            _pushServiceClient = pushServiceClient ?? throw new System.ArgumentNullException(nameof(pushServiceClient));
        }

        public VapidAuthentication DefaultAuthentication
        {
            get => _pushServiceClient.DefaultAuthentication;
            set => _pushServiceClient.DefaultAuthentication = value;
        }

        public Task RequestPushMessageDeliveryAsync(
            PushSubscription subscription, 
            PushMessage pushMessage, 
            CancellationToken cancellationToken = default)
        {
            return _pushServiceClient.RequestPushMessageDeliveryAsync(subscription, pushMessage, cancellationToken);
        }
    }
} 