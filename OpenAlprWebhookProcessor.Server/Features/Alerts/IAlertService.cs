namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public interface IAlertService
    {
        void AddJob(AlertUpdateRequest request);
    }
} 