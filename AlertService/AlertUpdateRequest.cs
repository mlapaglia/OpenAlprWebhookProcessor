namespace OpenAlprWebhookProcessor.AlertService
{
    public class AlertUpdateRequest
    {
        public long CameraId { get; set; }

        public string Description { get; set; }

        public string OpenAlprGroupUuid { get; set; }
    }
}
