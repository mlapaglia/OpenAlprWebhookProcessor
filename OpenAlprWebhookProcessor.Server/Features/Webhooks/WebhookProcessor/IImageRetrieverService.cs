using System.Collections.Generic;
using System.Threading;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public interface IImageRetrieverService
    {
        void AddImageRetrievalJob(string uuid);

        void AddImageCompressionJob(string ignoreThisParameter);

        void AddImageCompressionJob();

        void RemoveImageRequest(string uuid);

        int GetImageRequestsCount();

        int GetCompressionRequestsCount();

        IAsyncEnumerable<string> GetConsumingImageRequestsAsync(CancellationToken cancellationToken = default);

        IAsyncEnumerable<string> GetConsumingCompressionRequestsAsync(CancellationToken cancellationToken = default);
    }
}