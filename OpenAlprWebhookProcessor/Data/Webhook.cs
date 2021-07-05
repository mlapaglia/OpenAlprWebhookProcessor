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

        public string BestNumber { get; set; }

        public string PossibleNumbers { get; set; }

        public string Jpeg { get; set; }

        public double Confidence { get; set; }

        public string VehicleColor { get; set; }

        public string VehicleMake { get; set; }

        public string VehicleMakeModel { get; set; }

        public string VehicleType { get; set; }

        public string VehicleYear { get; set; }

        public string VehicleRegion { get; set; }

        public double Direction { get; set; }

        public string PlateCoordinates { get; set; }

        public string Notes { get; set; }
    }
}
