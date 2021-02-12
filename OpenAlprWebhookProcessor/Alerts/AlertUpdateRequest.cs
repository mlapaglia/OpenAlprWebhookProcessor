using System;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class AlertUpdateRequest
    {
        public Guid CameraId { get; set; }

        public string Description { get; set; }

        public Guid LicensePlateId { get; set; }

        public bool IsStrictMatch { get; set; }
    }
}
