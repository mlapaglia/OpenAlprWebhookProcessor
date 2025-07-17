namespace OpenAlprWebhookProcessor.Alerts
{
    public interface IAlertService
    {
        void AddJob(AlertUpdateRequest request);
    }
} 