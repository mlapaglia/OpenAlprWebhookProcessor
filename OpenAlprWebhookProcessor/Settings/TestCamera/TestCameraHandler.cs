using System;

namespace OpenAlprWebhookProcessor.Settings.TestCamera
{
    public class TestCameraHandler
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public TestCameraHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public void SendTestCameraOverlay(Guid cameraId)
        {
            _cameraUpdateService.ScheduleOverlayRequest(new CameraUpdateService.CameraUpdateRequest()
            {
                Id = cameraId,
                LicensePlate = "test",
                AlertDescription = "test",
                OpenAlprProcessingTimeMs = 1000,
                ProcessedPlateConfidence = 100,
                VehicleDescription = "test vehicle"
            });
        }
    }
}