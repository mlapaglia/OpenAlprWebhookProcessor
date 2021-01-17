namespace OpenAlprWebhookProcessor.Settings.TestCamera
{
    public class TestCameraHandler
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public TestCameraHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public void SendTestCameraOverlay(long cameraId)
        {
            _cameraUpdateService.AddJob(new CameraUpdateService.CameraUpdateRequest()
            {
                LicensePlate = "test",
                AlertDescription = "test",
                OpenAlprCameraId = cameraId,
                OpenAlprProcessingTimeMs = 1000,
                ProcessedPlateConfidence = 100,
                VehicleDescription = "test vehicle"
            });
        }
    }
}