using System;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class AlertUpdateRequest
    {
        public string Description { get; set; }

        public bool IsUrgent { get; set; }

        public string PlateNumber { get; set; }

        public byte[] PlateJpeg { get; set; }

        public DateTimeOffset ReceivedOn { get; set; }
    }
}
