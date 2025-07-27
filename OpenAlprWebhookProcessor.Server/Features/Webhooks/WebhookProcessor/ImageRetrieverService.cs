using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public class ImageRetrieverService : IImageRetrieverService, IDisposable
    {
        private readonly Channel<string> _imageRequestsChannel;
        private readonly ChannelWriter<string> _imageRequestsWriter;
        private readonly ChannelReader<string> _imageRequestsReader;

        private readonly Channel<string> _imageCompressionRequestsChannel;
        private readonly ChannelWriter<string> _imageCompressionRequestsWriter;
        private readonly ChannelReader<string> _imageCompressionRequestsReader;

        private readonly HashSet<string> _imageRequestsToProcessList = new();
        private readonly Lock _imageRequestsToProcessGate = new();
        private readonly HashSet<string> _imageCompressionRequestsToProcessList = new();
        private readonly Lock _imageCompressionRequestsGate = new();

        private bool _disposed = false;

        public ImageRetrieverService()
        {
            _imageRequestsChannel = Channel.CreateUnbounded<string>();
            _imageRequestsWriter = _imageRequestsChannel.Writer;
            _imageRequestsReader = _imageRequestsChannel.Reader;

            _imageCompressionRequestsChannel = Channel.CreateUnbounded<string>();
            _imageCompressionRequestsWriter = _imageCompressionRequestsChannel.Writer;
            _imageCompressionRequestsReader = _imageCompressionRequestsChannel.Reader;
        }

        public void AddImageRetrievalJob(string uuid)
        {
            if (_disposed || string.IsNullOrWhiteSpace(uuid))
                return;

            lock (_imageRequestsToProcessGate)
            {
                if (_imageRequestsToProcessList.Add(uuid))
                {
                    if (!_imageRequestsWriter.TryWrite(uuid))
                    {
                        _imageRequestsToProcessList.Remove(uuid);
                    }
                }
            }
        }

        public void AddImageCompressionJob(string ignoreThisParameter)
        {
            if (_disposed || string.IsNullOrWhiteSpace(ignoreThisParameter))
                return;

            lock (_imageCompressionRequestsGate)
            {
                if (_imageCompressionRequestsToProcessList.Add(ignoreThisParameter))
                {
                    if (!_imageCompressionRequestsWriter.TryWrite(ignoreThisParameter))
                    {
                        _imageCompressionRequestsToProcessList.Remove(ignoreThisParameter);
                    }
                }
            }
        }

        public void AddImageCompressionJob()
        {
            if (_disposed)
                return;

            lock (_imageCompressionRequestsGate)
            {
                _imageCompressionRequestsWriter.TryWrite("allImages");
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
            return _imageRequestsChannel.Reader.Count;
        }

        public int GetCompressionRequestsCount()
        {
            return _imageCompressionRequestsChannel.Reader.Count;
        }

        public async IAsyncEnumerable<string> GetConsumingImageRequestsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var request in _imageRequestsReader.ReadAllAsync(cancellationToken))
            {
                yield return request;
            }
        }

        public async IAsyncEnumerable<string> GetConsumingCompressionRequestsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var request in _imageCompressionRequestsReader.ReadAllAsync(cancellationToken))
            {
                yield return request;
            }
        }

        public void CompleteImageRequests()
        {
            _imageRequestsWriter.TryComplete();
        }

        public void CompleteCompressionRequests()
        {
            _imageCompressionRequestsWriter.TryComplete();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            CompleteImageRequests();
            CompleteCompressionRequests();
            _disposed = true;
        }
    }
}