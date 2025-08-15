using System.ComponentModel.DataAnnotations;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotRelay
{
    public class SnapshotRelayConfiguration
    {
        [Range(1, int.MaxValue, ErrorMessage = $"{nameof(TimeoutMilliseconds)} must be greater than zero.")]
        public int TimeoutMilliseconds { get; set; }
    }
}
