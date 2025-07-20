using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Utilities;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class GroupWebhookHandler : IGroupWebhookHandler
    {
        private readonly ILogger _logger;

        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        private readonly ICameraUpdateService _cameraUpdateService;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IAlertService _alertService;

        private readonly IImageRetrieverService _imageRetrieverService;

        public GroupWebhookHandler(
            ILogger<GroupWebhookHandler> logger,
            ICameraUpdateService cameraUpdateService,
            IUnitOfWork unitOfWork,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub,
            IAlertService alertService,
            IImageRetrieverService imageRetrieverService)
        {
            _logger = logger;
            _cameraUpdateService = cameraUpdateService;
            _unitOfWork = unitOfWork;
            _processorHub = processorHub;
            _alertService = alertService;
            _imageRetrieverService = imageRetrieverService;
        }

        public async Task HandleWebhookAsync(
            OpenAlprWebhook.Webhook webhook,
            bool isBulkImport,
            CancellationToken cancellationToken)
        {
            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            PlateGroupRaw rawDebugPlateGroup = null;

            if (agent.IsDebugEnabled)
            {
                rawDebugPlateGroup = new PlateGroupRaw
                {
                    PlateGroupId = webhook.Group.BestUuid,
                    ReceivedOnEpoch = webhook.Group.EpochStart,
                    RawPlateGroup = JsonSerializer.Serialize(webhook),
                    WasProcessedCorrectly = false,
                };

                await _unitOfWork.RawPlateGroups.AddAsync(
                    rawDebugPlateGroup,
                    cancellationToken);

                 await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var camera = await _unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.OpenAlprCameraId == webhook.Group.CameraId,
                cancellationToken);

            if (camera == null)
            {
                _logger.LogError("unknown camera: {CameraId}, skipping.", webhook.Group.CameraId);
                return;
            }

            if (!camera.OpenAlprEnabled)
            {
                _logger.LogError("camera has OpenALPR integration disabled, skipping.");
                return;
            }

            if (webhook.Group.IsParked)
            {
                _logger.LogInformation("parked car: {PlateNumber}, ignoring.", webhook.Group.BestPlateNumber);
                return;
            }

            var previousPreviewGroups = await _unitOfWork.PlateGroups.GetQueryable()
                .Where(x => webhook.Group.Uuids.Contains(x.OpenAlprUuid))
                .ToListAsync(cancellationToken);

            Data.PlateGroup plateGroup;
            if (previousPreviewGroups.Count > 0)
            {
                plateGroup = previousPreviewGroups[0];
                _unitOfWork.PlateGroups.DeleteRange(previousPreviewGroups.Skip(1));

                _logger.LogInformation("Previous preview plate exists: {PlateNumber}, overwriting", plateGroup.BestNumber);
            }
            else
            {
                plateGroup = new Data.PlateGroup();
            }

            plateGroup.AlertDescription = webhook.Description;
            plateGroup.PlateCoordinates = FormatLicensePlateXyCoordinates(webhook.Group.BestPlate.Coordinates);
            plateGroup.Direction = webhook.Group.TravelDirection;
            plateGroup.IsAlert = webhook.DataType == "alpr_alert";
            plateGroup.OpenAlprCameraId = webhook.Group.CameraId;
            plateGroup.OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2);
            plateGroup.OpenAlprUuid = webhook.Group.BestUuid;
            plateGroup.BestNumber = webhook.Group.BestPlateNumber;
            plateGroup.PossibleNumbers = webhook.Group.Candidates.Select(x => new PlateGroupPossibleNumbers() { Number = x.Plate }).ToList();
            plateGroup.Confidence = Math.Round(webhook.Group.BestPlate.Confidence, 2);
            plateGroup.ReceivedOnEpoch = webhook.Group.EpochStart;

            MapVehicle(plateGroup, webhook);

            if (previousPreviewGroups.Count == 0)
            {
                await _unitOfWork.PlateGroups.AddAsync(plateGroup, cancellationToken);
            }

            if (rawDebugPlateGroup != null)
            {
                rawDebugPlateGroup.WasProcessedCorrectly = true;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("plate saved successfully");

            _imageRetrieverService.AddImageRetrievalJob(plateGroup.OpenAlprUuid);

            if (!isBulkImport)
            {
                var plateJpeg = webhook.Group.BestPlate != null ? Convert.FromBase64String(webhook.Group.BestPlate.PlateCropJpeg) : null;

                if (camera.UpdateOverlayEnabled)
                {
                    var updateRequest = new CameraUpdateRequest()
                    {
                        LicensePlateImageUuid = webhook.Group.BestUuid,
                        LicensePlate = webhook.Group.BestPlateNumber,
                        LicensePlateJpeg = plateJpeg,
                        Id = camera.Id,
                        OpenAlprProcessingTimeMs = webhook.Group.BestPlate != null ? Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2) : 0,
                        ProcessedPlateConfidence = webhook.Group.BestPlate != null ? Math.Round(webhook.Group.BestPlate.Confidence, 2) : 0,
                        IsAlert = webhook.DataType == "alpr_alert",
                        IsPreviewGroup = webhook.Group.IsPreview,
                        AlertDescription = webhook.Description,
                        VehicleDescription = VehicleUtilities.FormatVehicleDescription(plateGroup.VehicleYear + " " + plateGroup.VehicleMakeModel),
                    };

                    await _cameraUpdateService.ScheduleOverlayRequestAsync(updateRequest);
                }

                if (!webhook.Group.IsPreview)
                {
                    await _processorHub.Clients.All.LicensePlateRecorded(webhook.Group.BestPlateNumber);

                    var alerts = (await _unitOfWork.Alerts.GetAllAsync(cancellationToken)).ToList();

                    var alert = alerts.FirstOrDefault(x =>
                        x.PlateNumber.ToUpper() == webhook.Group.BestPlateNumber
                        || plateGroup.PossibleNumbers.Any(y => y.Number == x.PlateNumber.ToUpper()));

                    var receivedOn = DateTimeOffset.FromUnixTimeMilliseconds(webhook.Group.EpochStart);

                    if (alert != null)
                    {
                        var alertUpdateRequest = new AlertUpdateRequest()
                        {
                            Description = $"{alert.PlateNumber} {alert.Description} was seen on {receivedOn:g}",
                            IsUrgent = true,
                            PlateId = plateGroup.Id,
                            PlateJpeg = plateJpeg,
                            PlateJpegUrl = $"/api/images/crop/{plateGroup.OpenAlprUuid}",
                            PlateNumber = alert.PlateNumber,
                            ReceivedOn = receivedOn,
                        };

                        _alertService.AddJob(alertUpdateRequest);
                    }
                    else
                    {
                        var alertUpdateRequest = new AlertUpdateRequest()
                        {
                            Description = $"{plateGroup.BestNumber} was seen on {receivedOn:g}",
                            IsUrgent = false,
                            PlateId = plateGroup.Id,
                            PlateJpeg = plateJpeg,
                            PlateJpegUrl = $"/api/images/crop/{plateGroup.OpenAlprUuid}",
                            PlateNumber = plateGroup.BestNumber,
                            ReceivedOn = receivedOn,
                        };

                        _alertService.AddJob(alertUpdateRequest);
                    }
                }

                var forwards = await _unitOfWork.WebhookForwards.GetAllAsync(cancellationToken);

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
                            _logger.LogError(ex, "failed to forward webhook to: {Url}, error: {Error}", forward.FowardingDestination, ex.Message);
                        }
                    }
                }
            }
        }

        private static void MapVehicle(
            Data.PlateGroup plateGroup,
            OpenAlprWebhook.Webhook webhook)
        {
            if (webhook.Group.Vehicle?.MakeModels.First()?.Confidence > 30)
            {
                plateGroup.VehicleMakeModel = webhook.Group.Vehicle.MakeModels.First()?.Name;

                if (webhook.Group.Vehicle.Colors.First()?.Confidence > 30)
                {
                    plateGroup.VehicleColor = webhook.Group.Vehicle.Colors.First()?.Name;
                }

                if (webhook.Group.Vehicle.Makes.First()?.Confidence > 30)
                {
                    plateGroup.VehicleMake = webhook.Group.Vehicle.Makes.First()?.Name;
                }

                if (webhook.Group.Vehicle.BodyTypes.First()?.Confidence > 30)
                {
                    plateGroup.VehicleType = webhook.Group.Vehicle.BodyTypes.First()?.Name;
                }

                if (webhook.Group.Vehicle.Years.First()?.Confidence > 30)
                {
                    plateGroup.VehicleYear = webhook.Group.Vehicle.Years.First()?.Name;
                }

                if (webhook.Group.BestPlate.RegionConfidence > 30)
                {
                    plateGroup.VehicleRegion = webhook.Group.BestPlate.Region;
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