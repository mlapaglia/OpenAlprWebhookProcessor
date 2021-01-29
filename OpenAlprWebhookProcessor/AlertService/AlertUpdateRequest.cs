using System;

namespace OpenAlprWebhookProcessor.AlertService
{
    public class AlertUpdateRequest
    {
        public Guid CameraId { get; set; }

        public string Description { get; set; }

        public Guid LicensePlateId { get; set; }
    }
}
