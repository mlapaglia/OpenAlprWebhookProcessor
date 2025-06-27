using System;

namespace OpenAlprWebhookProcessor.Server.Data
{
    public class WebPushSubscriptionKey
    {
        public Guid Id { get; set; }

        public WebPushSubscription MobilePushSubscription { get; set; }

        public Guid MobilePushSubscriptionId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
