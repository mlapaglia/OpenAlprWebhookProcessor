using Lib.Net.Http.WebPush;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions
{
    public interface IWebPushSubscriptionsService
    {
        List<PushSubscription> GetAll();

        void Insert(PushSubscription subscription);

        void Delete(string endpoint);
    }
}