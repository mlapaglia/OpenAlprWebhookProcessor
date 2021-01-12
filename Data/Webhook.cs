using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class PlateGroup
    {
        public Guid Id { get; set; }

        public string OpenAlprUuid { get; set; }

        public int OpenAlprCameraId { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public bool IsAlert { get; set; }

        public string AlertDescription { get; set; }

        public long ReceivedOnEpoch { get; set; }

        public string Number { get; set; }

        public string Jpeg { get; set; }

        public double Confidence { get; set; }

        public string VehicleDescription { get; set; }

        public double Direction { get; set; }

        public string PlateCoordinates { get; set; }
    }
}
