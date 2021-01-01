using System;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookHandler
    {
        private readonly ILogger _logger;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public WebhookHandler(
            ILogger<WebhookHandler> logger,
            CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _logger = logger;
            _cameraUpdateService = cameraUpdateService;
        }

        public void Handle(OpenAlprWebhook webhook)
        {
            var updateRequest = new CameraUpdateRequest()
            {
                LicensePlate = webhook.BestPlateNumber,
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprProcessingTimeMs = Math.Round(webhook.BestPlate.ProcessingTimeMs, 2),
                ProcessedPlateConfidence = Math.Round(webhook.BestPlate.Confidence, 2),
                VehicleDescription = $"{webhook.Vehicle.Year[0]?.Name} {webhook.Vehicle.MakeModel[0]?.Name}",
            };

            _cameraUpdateService.AddJob(updateRequest);
        }
    }
}
