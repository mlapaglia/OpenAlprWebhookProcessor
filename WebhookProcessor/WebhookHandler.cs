using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Utilities;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookHandler
    {
        private readonly ILogger _logger;

        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        public WebhookHandler(
            ILogger<WebhookHandler> logger,
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            ProcessorContext processorContext,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub)
        {
            _logger = logger;
            _cameraUpdateService = cameraUpdateService;
            _processorContext = processorContext;
            _processorHub = processorHub;
        }

        public async Task HandleWebhookAsync(Webhook webhook)
        {
            var updateRequest = new CameraUpdateRequest()
            {
                LicensePlateImageUuid = webhook.Group.BestUuid,
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
                updateRequest.VehicleDescription = $"{webhook.Group.Vehicle.Year[0].Name} {VehicleUtilities.FormatVehicleDescription(webhook.Group.Vehicle.MakeModel[0].Name)}";
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

            await _processorHub.Clients.All.LicenePlateRecorded(updateRequest.LicensePlate);
        }

        public static string FormatLicensePlateXyCoordinates(List<Coordinate> coordinates)
        {
            return VehicleUtilities.FormatLicensePlateImageCoordinates(
                new List<int>()
                {
                    coordinates[0].X,
                    coordinates[1].X,
                    coordinates[2].X,
                    coordinates[3].X,
                },
                new List<int>()
                {
                    coordinates[0].Y,
                    coordinates[1].Y,
                    coordinates[2].Y,
                    coordinates[3].Y,
                });
        }
    }
}