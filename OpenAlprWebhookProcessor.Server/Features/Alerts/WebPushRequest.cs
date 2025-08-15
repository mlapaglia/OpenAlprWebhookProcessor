namespace OpenAlprWebhookProcessor.Alerts.WebPush
{
    public class WebPushRequest
    {
        public bool IsEnabled { get; set; }
        
        public bool SendEveryPlateEnabled { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public string EmailAddress { get; set; }
    }
}
