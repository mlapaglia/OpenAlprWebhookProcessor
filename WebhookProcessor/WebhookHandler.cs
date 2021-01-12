using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookHandler
    {
        private readonly ILogger _logger;

        private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        public WebhookHandler(
            ILogger<WebhookHandler> logger,
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            ProcessorContext processorContext)
        {
            _logger = logger;
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
                AlertDescription = webhook.Description,
                PlateCoordinates = FormatLicensePlateXyCoordinates(webhook.Group.BestPlate.Coordinates),
                Direction = webhook.Group.TravelDirection,
                IsAlert = webhook.DataType == "alpr_alert",
                OpenAlprCameraId = webhook.Group.CameraId,
                OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2),
                OpenAlprUuid = webhook.Group.BestUuid,
                Number = webhook.Group.BestPlateNumber,
                Jpeg = webhook.Group.BestPlate.PlateCropJpeg,
                Confidence = Math.Round(webhook.Group.BestPlate.Confidence, 2),
                ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                VehicleDescription = updateRequest.VehicleDescription,
            });

            await _processorContext.SaveChangesAsync();

            _logger.LogInformation("plate saved successfully");
        }

        private string FormatVehicleDescription(string vehicleMakeModel)
        {
            return _textInfo
                .ToTitleCase(vehicleMakeModel.Replace('_', ' '));
        }

        private static string FormatLicensePlateXyCoordinates(List<Coordinate> coordinates)
        {
            return $"x1={coordinates[0].X}&y1={coordinates[0].Y}&x2={coordinates[1].X}y2={coordinates[1].Y}";
        }
    }
}