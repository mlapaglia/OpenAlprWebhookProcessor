using System;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public class AlertUpdateRequest
    {
        public string Description { get; set; }

        public bool IsUrgent { get; set; }

        public Guid PlateId { get; set; }

        public string PlateNumber { get; set; }

        public byte[] PlateJpeg { get; set; }

        public string PlateJpegUrl { get; set; }

        public DateTimeOffset ReceivedOn { get; set; }
    }
}
