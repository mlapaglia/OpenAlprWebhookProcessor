namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateRequest
    {
        public int OpenAlprCameraId { get; set; }

        public string VehicleDescription { get; set; }

        public string LicensePlate { get; set; }
    }
}
