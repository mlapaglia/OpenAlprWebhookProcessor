using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    public class LicensePlate
    {
        public Guid Id { get; set; }

        public int OpenAlprCameraId { get; set; }

        public string VehicleDescription { get; set; }

        public string PlateNumber { get; set; }

        public string PossiblePlateNumbers { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public double ProcessedPlateConfidence { get; set; }

        public bool IsAlert { get; set; }

        public bool IsIgnore { get; set; }

        public string AlertDescription { get; set; }

        public DateTimeOffset ReceivedOn { get; set; }

        public double Direction { get; set; }

        public Uri ImageUrl { get; set; }

        public Uri CropImageUrl { get; set; }
    }
}
