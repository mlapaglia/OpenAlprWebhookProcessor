using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor
{
    public interface IImageRetrieverService
    {
        void AddImageCompressionJob();
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        bool TryAddJob(string openAlprImageId);
    }
}