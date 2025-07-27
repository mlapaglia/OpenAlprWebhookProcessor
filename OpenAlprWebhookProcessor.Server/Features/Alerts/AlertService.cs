using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public class AlertService : IAlertService, IDisposable
    {
        private readonly Channel<AlertUpdateRequest> _alertsChannel;
        private readonly ChannelWriter<AlertUpdateRequest> _writer;
        private readonly ChannelReader<AlertUpdateRequest> _reader;

        private bool _disposed = false;

        public AlertService()
        {
            // Create an unbounded channel for alert requests
            _alertsChannel = Channel.CreateUnbounded<AlertUpdateRequest>();
            _writer = _alertsChannel.Writer;
            _reader = _alertsChannel.Reader;
        }

        public void AddJob(AlertUpdateRequest request)
        {
            if (_disposed)
                return;

            if (!_writer.TryWrite(request))
            {
                // Log warning if needed - channel is closed
                // Could throw exception or handle gracefully based on your needs
            }
        }

        public int GetPendingAlertsCount()
        {
            return _reader.Count;
        }

        public async IAsyncEnumerable<AlertUpdateRequest> GetConsumingAlertsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var alert in _reader.ReadAllAsync(cancellationToken))
            {
                yield return alert;
            }
        }

        public void CompleteChannel()
        {
            _writer.TryComplete();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            CompleteChannel();
            _disposed = true;
        }
    }
}