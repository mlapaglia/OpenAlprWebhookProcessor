using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Data
{
    public class WebPushSubscription
    {
        public Guid Id { get; set; }

        public string Endpoint { get; set; }

        public List<WebPushSubscriptionKey> Keys { get; set; }
    }
}
