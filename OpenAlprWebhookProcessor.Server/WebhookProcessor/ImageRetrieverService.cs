using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class ImageRetrieverService : IImageRetrieverService
    {
        private readonly BlockingCollection<string> _imageRequestsToProcess = new BlockingCollection<string>();

        private readonly HashSet<string> _imageRequestsToProcessList = new();

        private readonly object _imageRequestsToProcessGate = new();

        private readonly BlockingCollection<string> _imageCompressionRequestsToProcess = new();

        private readonly HashSet<string> _imageCompressionRequestsToProcessList = new();

        private readonly object _imageCompressionRequestsGate = new();

        public void AddImageRetrievalJob(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return;

            lock (_imageRequestsToProcessGate)
            {
                if (_imageRequestsToProcessList.Add(uuid))
                {
                    _imageRequestsToProcess.Add(uuid);
                }
            }
        }

        public void AddImageCompressionJob(string ignoreThisParameter)
        {
            if (string.IsNullOrWhiteSpace(ignoreThisParameter))
                return;

            lock (_imageCompressionRequestsGate)
            {
                if (_imageCompressionRequestsToProcessList.Add(ignoreThisParameter))
                {
                    _imageCompressionRequestsToProcess.Add(ignoreThisParameter);
                }
            }
        }

        public void AddImageCompressionJob()
        {
            lock (_imageCompressionRequestsGate)
            {
                _imageCompressionRequestsToProcess.Add("allImages");
            }
        }

        public void RemoveImageRequest(string uuid)
        {
            lock (_imageRequestsToProcessGate)
            {
                _imageRequestsToProcessList.Remove(uuid);
            }
        }

        public int GetImageRequestsCount()
        {
            return _imageRequestsToProcess.Count;
        }

        public int GetCompressionRequestsCount()
        {
            return _imageCompressionRequestsToProcess.Count;
        }

        public IEnumerable<string> GetConsumingImageRequests(CancellationToken cancellationToken)
        {
            return _imageRequestsToProcess.GetConsumingEnumerable(cancellationToken);
        }

        public IEnumerable<string> GetConsumingCompressionRequests(CancellationToken cancellationToken)
        {
            return _imageCompressionRequestsToProcess.GetConsumingEnumerable(cancellationToken);
        }
    }
}