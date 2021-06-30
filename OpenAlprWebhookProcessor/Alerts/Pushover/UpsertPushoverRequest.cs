namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class UpsertPushoverRequest
    {
        public bool IsEnabled { get; set; }

        public string UserKey { get; set; }

        public string ApiToken { get; set; }

        public bool SendPlatePreview { get; set; }
    }
}
