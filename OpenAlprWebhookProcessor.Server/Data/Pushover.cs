using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class Pushover
    {
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public string UserKey { get; set; }

        public string ApiToken { get; set; }

        public bool SendPlatePreview { get; set; }

        public bool SendEveryPlateEnabled { get; set; }
    }
}
