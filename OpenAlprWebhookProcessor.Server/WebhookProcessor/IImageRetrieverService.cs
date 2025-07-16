namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public interface IImageRetrieverService
    {
        void AddImageRetrievalJob(string uuid);
        void AddImageCompressionJob(string ignoreThisParameter);
        void AddImageCompressionJob();
    }
} 