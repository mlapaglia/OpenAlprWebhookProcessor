using Lib.Net.Http.WebPush;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.PushSubscriptions
{
    public interface IPushSubscriptionsService
    {
        List<PushSubscription> GetAll();

        void Insert(PushSubscription subscription);

        void Delete(string endpoint);
    }
}