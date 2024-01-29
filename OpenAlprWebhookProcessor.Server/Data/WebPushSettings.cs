using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class WebPushSettings
    {
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public string Subject { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public bool SendEveryPlateEnabled { get; set; }
    }
}
