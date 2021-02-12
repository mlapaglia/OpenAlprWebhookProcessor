using OpenAlprWebhookProcessor.CameraUpdateService;
using System;

namespace OpenAlprWebhookProcessor.Cameras
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
                IsTest = true,
                LicensePlate = "test",
                AlertDescription = "test",
                OpenAlprProcessingTimeMs = 1000,
                ProcessedPlateConfidence = 100,
                VehicleDescription = "test vehicle"
            });
        }
        
        public void SendNightModeCommand(
            Guid cameraId,
            SunriseSunset sunriseSunset)
        {
            _cameraUpdateService.EnqueueDayNight(
                cameraId,
                sunriseSunset);
        }
    }
}