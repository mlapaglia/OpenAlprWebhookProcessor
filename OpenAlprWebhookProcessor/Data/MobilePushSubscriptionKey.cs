using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class MobilePushSubscriptionKey
    {
        public Guid Id { get; set; }

        public MobilePushSubscription MobilePushSubscription { get; set; }

        public Guid MobilePushSubscriptionId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
