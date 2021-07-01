namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class PushoverRequest
    {
        public bool IsEnabled { get; set; }

        public string UserKey { get; set; }

        public string ApiToken { get; set; }

        public bool SendPlatePreviewEnabled { get; set; }
    }
}
