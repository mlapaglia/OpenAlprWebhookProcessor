using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class PlateGroupRaw
    {
        public Guid Id { get; set; }

        public string PlateGroupId { get; set; }

        public long ReceivedOnEpoch { get; set; }

        public string RawPlateGroup { get; set; }

        public bool WasProcessedCorrectly { get; set; }
    }
}
