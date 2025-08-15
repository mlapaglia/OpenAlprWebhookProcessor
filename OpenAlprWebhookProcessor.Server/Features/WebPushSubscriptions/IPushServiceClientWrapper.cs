using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions
{
    public interface IPushServiceClientWrapper
    {
        VapidAuthentication DefaultAuthentication { get; set; }
        
        Task RequestPushMessageDeliveryAsync(
            PushSubscription subscription, 
            PushMessage pushMessage, 
            CancellationToken cancellationToken = default);
    }
} 