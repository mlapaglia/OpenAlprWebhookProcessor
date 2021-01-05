using System;
using System.Globalization;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookHandler
    {
        private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        public WebhookHandler(
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            ProcessorContext processorContext)
        {
            _cameraUpdateService = cameraUpdateService;
            _processorContext = processorContext;
        }

        public async Task HandleWebhookAsync(Webhook webhook)
        {
            var updateRequest = new CameraUpdateRequest()
            {
                LicensePlate = webhook.Group.BestPlateNumber,
                LicensePlateJpeg = Convert.FromBase64String(webhook.Group.BestPlate.PlateCropJpeg),
                OpenAlprCameraId = webhook.Group.CameraId,
                OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2),
                ProcessedPlateConfidence = Math.Round(webhook.Group.BestPlate.Confidence, 2),
                IsAlert = webhook.DataType == "alpr_alert",
                AlertDescription = webhook.Description,
            };

            if (webhook.Group.Vehicle != null)
            {
                updateRequest.VehicleDescription = $"{webhook.Group.Vehicle.Year[0].Name} {FormatVehicleDescription(webhook.Group.Vehicle.MakeModel[0].Name)}";
            } 

            _cameraUpdateService.AddJob(updateRequest);
            _processorContext.PlateGroups.Add(new PlateGroup()
            {
                PlateNumber = webhook.Group.BestPlateNumber,
                PlateJpeg = webhook.Group.BestPlate.PlateCropJpeg,
                OpenAlprCameraId = webhook.Group.CameraId,
                OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2),
                PlateConfidence = Math.Round(webhook.Group.BestPlate.Confidence, 2),
                IsAlert = webhook.DataType == "alpr_alert",
                AlertDescription = webhook.Description
            });

            await _processorContext.SaveChangesAsync();
        }

        private string FormatVehicleDescription(string vehicleMakeModel)
        {
            return _textInfo
                .ToTitleCase(vehicleMakeModel.Replace('_', ' '));
        }
    }
}