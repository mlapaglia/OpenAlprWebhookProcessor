using System;

namespace OpenAlprWebhookProcessor.ProcessorHub
{
    public class SignalRConnectionInfo
    {
        public string ConnectionId { get; set; }

        public string UserId { get; set; }

        public DateTime ConnectedAt { get; set; }

        public string Transport { get; set; }

        public string UserAgent { get; set; }

        public string IpAddress { get; set; }
    }
}
