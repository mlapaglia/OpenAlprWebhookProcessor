using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Data
{
    public class MobilePushSubscription
    {
        public Guid Id { get; set; }

        public string Endpoint { get; set; }

        public List<MobilePushSubscriptionKey> Keys { get; set; }
    }
}
