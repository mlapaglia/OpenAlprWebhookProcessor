using System;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    public class LicensePlate
    {
        public int OpenAlprCameraId { get; set; }

        public string VehicleDescription { get; set; }

        public string PlateNumber { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public double ProcessedPlateConfidence { get; set; }

        public string LicensePlateJpegBase64 { get; set; }

        public bool IsAlert { get; set; }

        public string AlertDescription { get; set; }

        public DateTimeOffset ReceivedOn { get; set; }

        public double Direction { get; set; }

        public Uri ImageUrl { get; set; }

        public Uri CropImageUrl { get; set; }
    }
}
