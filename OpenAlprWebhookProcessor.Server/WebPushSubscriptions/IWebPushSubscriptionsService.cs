using Lib.Net.Http.WebPush;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions
{
    public interface IWebPushSubscriptionsService
    {
        Task<List<PushSubscription>> GetAllAsync(CancellationToken cancellationToken = default);

        Task InsertAsync(PushSubscription subscription, 
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string endpoint,
            CancellationToken cancellationToken = default);
    }
}