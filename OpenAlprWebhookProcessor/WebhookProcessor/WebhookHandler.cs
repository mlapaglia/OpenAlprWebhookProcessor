using System;
using System.Globalization;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookHandler
    {
        private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public WebhookHandler(
            CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public void Handle(OpenAlprWebhook webhook)
        {
            var updateRequest = new CameraUpdateRequest()
            {
                LicensePlate = webhook.BestPlateNumber,
                LicensePlateJpeg = Convert.FromBase64String(webhook.BestPlate.PlateCropJpeg),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprProcessingTimeMs = Math.Round(webhook.BestPlate.ProcessingTimeMs, 2),
                ProcessedPlateConfidence = Math.Round(webhook.BestPlate.Confidence, 2),
            };

            if (webhook.Vehicle.MakeModel != null && webhook.Vehicle.MakeModel.Count > 0)
            {
                updateRequest.VehicleDescription = $"{webhook.Vehicle.Year[0].Name} {FormatVehicleDescription(webhook.Vehicle.MakeModel[0].Name)}";
            } 

            _cameraUpdateService.AddJob(updateRequest);
        }

        private string FormatVehicleDescription(string vehicleMakeModel)
        {
            return _textInfo
                .ToTitleCase(vehicleMakeModel.Replace('_', ' '));
        }
    }
}