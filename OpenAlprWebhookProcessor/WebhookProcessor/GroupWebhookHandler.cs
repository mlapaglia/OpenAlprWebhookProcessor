using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Alerts;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Utilities;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class GroupWebhookHandler
    {
        private readonly ILogger _logger;

        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        private readonly AlertService _alertService;

        public GroupWebhookHandler(
            ILogger<GroupWebhookHandler> logger,
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            ProcessorContext processorContext,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub,
            AlertService alertService)
        {
            _logger = logger;
            _cameraUpdateService = cameraUpdateService;
            _processorContext = processorContext;
            _processorHub = processorHub;
            _alertService = alertService;
        }

        public async Task HandleWebhookAsync(
            Webhook webhook,
            CancellationToken cancellationToken)
        {
            var camera = await _processorContext.Cameras
                .Where(x => x.OpenAlprCameraId == webhook.Group.CameraId)
                .FirstOrDefaultAsync(cancellationToken);

            if (camera == null)
            {
                throw new ArgumentException("unknown camera, skipping");
            }

            if (!camera.OpenAlprEnabled)
            {
                throw new ArgumentException("camera has OpenALPR integration disabled, skipping");
            }

            if (!webhook.Group.IsParked)
            {
                _logger.LogInformation($"parked car: {webhook.Group.BestPlateNumber}, ignoring.");
                return;
            }

            string vehicleDescription = null;

            if (webhook.Group.Vehicle != null)
            {
                vehicleDescription = $"{webhook.Group.Vehicle.Year[0].Name} {VehicleUtilities.FormatVehicleDescription(webhook.Group.Vehicle.MakeModel[0].Name)}";
            }

            if (camera.UpdateOverlayEnabled)
            {
                var updateRequest = new CameraUpdateRequest()
                {
                    LicensePlateImageUuid = webhook.Group.BestUuid,
                    LicensePlate = webhook.Group.BestPlateNumber,
                    LicensePlateJpeg = Convert.FromBase64String(webhook.Group.BestPlate.PlateCropJpeg),
                    Id = camera.Id,
                    OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2),
                    ProcessedPlateConfidence = Math.Round(webhook.Group.BestPlate.Confidence, 2),
                    IsAlert = webhook.DataType == "alpr_alert",
                    IsPreviewGroup = webhook.Group.IsPreview,
                    AlertDescription = webhook.Description,
                    VehicleDescription = vehicleDescription,
                };

                _cameraUpdateService.ScheduleOverlayRequest(updateRequest);
            }

            if (!webhook.Group.IsPreview)
            {
                var plateGroup = new PlateGroup()
                {
                    AlertDescription = webhook.Description,
                    PlateCoordinates = FormatLicensePlateXyCoordinates(webhook.Group.BestPlate.Coordinates),
                    Direction = webhook.Group.TravelDirection,
                    IsAlert = webhook.DataType == "alpr_alert",
                    OpenAlprCameraId = webhook.Group.CameraId,
                    OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2),
                    OpenAlprUuid = webhook.Group.BestUuid,
                    BestNumber = webhook.Group.BestPlateNumber,
                    PossibleNumbers = string.Join(",", webhook.Group.Candidates.Select(x => x.Plate).ToArray()),
                    Jpeg = webhook.Group.BestPlate.PlateCropJpeg,
                    Confidence = Math.Round(webhook.Group.BestPlate.Confidence, 2),
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    VehicleDescription = vehicleDescription,
                };

                _processorContext.PlateGroups.Add(plateGroup);

                await _processorContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("plate saved successfully");

                await _processorHub.Clients.All.LicensePlateRecorded(webhook.Group.BestPlateNumber);

                var alert = await _processorContext.Alerts
                    .Where(x => x.PlateNumber == webhook.Group.BestPlateNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                if (alert != null)
                {
                    var alertUpdateRequest = new AlertUpdateRequest()
                    {
                        CameraId = camera.Id,
                        Description = alert.Description,
                        LicensePlateId = plateGroup.Id,
                        IsStrictMatch = alert.IsStrictMatch,
                    };

                    _alertService.AddJob(alertUpdateRequest);
                }
            }

            var forwards = await _processorContext.WebhookForwards.ToListAsync(cancellationToken);

            foreach (var forward in forwards)
            {
                if (forward.ForwardGroups || (forward.ForwardGroupPreviews && webhook.Group.IsPreview))
                {
                    try
                    {
                        await WebhookForwarder.ForwardWebhookAsync(
                            webhook,
                            forward.FowardingDestination,
                            forward.IgnoreSslErrors,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("failed to forward webhook to: {url}, error: {error}", forward.FowardingDestination, ex.Message);
                    }
                }
            }
        }

        private static string FormatLicensePlateXyCoordinates(List<Coordinate> coordinates)
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