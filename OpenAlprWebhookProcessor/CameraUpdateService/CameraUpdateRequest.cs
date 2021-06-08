using System;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateRequest
    {
        public Guid Id { get; set; }

        public string VehicleDescription { get; set; }

        public string LicensePlate { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public double ProcessedPlateConfidence { get; set; }

        public string LicensePlateImageUuid { get; set; }

        public byte[] LicensePlateJpeg { get; set; }

        public bool IsAlert { get; set; }

        public bool IsTest { get; set; }

        public bool IsSinglePlate { get; set; }

        public bool IsPreviewGroup { get; set; }

        public string AlertDescription { get; set; }
    }
}
