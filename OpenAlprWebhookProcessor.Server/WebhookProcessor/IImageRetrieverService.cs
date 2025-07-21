using System.Collections.Generic;
using System.Threading;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public interface IImageRetrieverService
    {
        void AddImageRetrievalJob(string uuid);

        void AddImageCompressionJob(string ignoreThisParameter);

        void AddImageCompressionJob();

        void RemoveImageRequest(string uuid);

        int GetImageRequestsCount();

        int GetCompressionRequestsCount();

        IEnumerable<string> GetConsumingImageRequests(CancellationToken cancellationToken);

        IEnumerable<string> GetConsumingCompressionRequests(CancellationToken cancellationToken);
    }
}