namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateRequest
    {
        public long OpenAlprCameraId { get; set; }

        public string VehicleDescription { get; set; }

        public string LicensePlate { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public double ProcessedPlateConfidence { get; set; }

        public string LicensePlateImageUuid { get; set; }

        public byte[] LicensePlateJpeg { get; set; }

        public bool IsAlert { get; set; }

        public string AlertDescription { get; set; }
    }
}
