using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Server.Alerts;
using OpenAlprWebhookProcessor.Server.CameraUpdateService;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.ProcessorHub;
using OpenAlprWebhookProcessor.Server.Utilities;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor
{
    public class GroupWebhookHandler : IGroupWebhookHandler
    {
        private readonly ILogger _logger;

        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        private readonly ICameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        private readonly IAlertService _alertService;

        private readonly IImageRetrieverService _imageRetrieverService;

        public GroupWebhookHandler(
            ILogger<GroupWebhookHandler> logger,
            ICameraUpdateService cameraUpdateService,
            ProcessorContext processorContext,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub,
            IAlertService alertService,
            IImageRetrieverService imageRetrieverService)
        {
            _logger = logger;
            _cameraUpdateService = cameraUpdateService;
            _processorContext = processorContext;
            _processorHub = processorHub;
            _alertService = alertService;
            _imageRetrieverService = imageRetrieverService;
        }

        /// <summary>
        /// Processes an incoming webhook from the OpenALPR Agent."/>
        /// </summary>
        /// <param name="webhook">The webhook from the Agent.</param>
        /// <param name="isBulkImport">Do not send alerts or notifications when a bulk import is occuring.</param>
        /// <param name="cancellationToken">Cancel the processing.</param>
        /// <returns>A task that completes when processing is finished.</returns>
        public async Task HandleWebhookAsync(
            Webhook webhook,
            bool isBulkImport,
            CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (agent == null)
            {
                _logger.LogError("agent missing, skipping. check agent settings.");
                return;
            }

            var rawDebugPlateGroup = await SaveRawPlateGroupAsync(
                webhook,
                agent,
                cancellationToken);

            var camera = await _processorContext.Cameras
                .AsNoTracking()
                .Where(x => x.OpenAlprCameraId == webhook.Group.CameraId)
                .FirstOrDefaultAsync(cancellationToken);

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

            var previousPreviewGroups = await _processorContext.PlateGroups
                .Where(x => webhook.Group.Uuids.Contains(x.OpenAlprUuid))
                .ToListAsync(cancellationToken);

            PlateGroup plateGroup;
            if (previousPreviewGroups.Count > 0)
            {
                plateGroup = previousPreviewGroups[0];
                _processorContext.PlateGroups.RemoveRange(previousPreviewGroups.Skip(1));

                _logger.LogInformation("Previous preview plate exists: {PlateNumber}, overwriting", plateGroup.BestNumber);
            }
            else
            {
                plateGroup = new PlateGroup();
            }

            MapPlateGroup(
                webhook,
                plateGroup);

            if (previousPreviewGroups.Count == 0)
            {
                _processorContext.PlateGroups.Add(plateGroup);
            }

            if (rawDebugPlateGroup != null)
            {
                rawDebugPlateGroup.WasProcessedCorrectly = true;
            }

            await _processorContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("plate saved successfully");

            _imageRetrieverService.TryAddJob(plateGroup.OpenAlprUuid);

            if (isBulkImport)
            {
                return;
            }

            var plateJpeg = webhook.Group.BestPlate != null ? Convert.FromBase64String(webhook.Group.BestPlate.PlateCropJpeg) : null;

            ScheduleUpdateCameraOverlay(
                webhook,
                camera,
                plateGroup,
                plateJpeg);

            await SendNotificationsAndAlertsAsync(
                webhook,
                plateGroup,
                plateJpeg,
                cancellationToken);

            await SendWebhookForwardsAsync(
                webhook,
                cancellationToken);
        }

        private async Task<PlateGroupRaw> SaveRawPlateGroupAsync(
            Webhook webhook,
            Agent agent,
            CancellationToken cancellationToken)
        {
            if (agent.IsDebugEnabled)
            {
                var rawDebugPlateGroup = new PlateGroupRaw
                {
                    PlateGroupId = webhook.Group.BestUuid,
                    ReceivedOnEpoch = webhook.Group.EpochStart,
                    RawPlateGroup = JsonSerializer.Serialize(webhook),
                    WasProcessedCorrectly = false,
                };

                _processorContext.RawPlateGroups.Add(rawDebugPlateGroup);
                await _processorContext.SaveChangesAsync(cancellationToken);

                return rawDebugPlateGroup;
            }

            return null;
        }

        private async Task SendWebhookForwardsAsync(Webhook webhook, CancellationToken cancellationToken)
        {
            var forwards = await _processorContext.WebhookForwards
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var forward in forwards)
            {
                if (forward.ForwardGroups || forward.ForwardGroupPreviews && webhook.Group.IsPreview)
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
                        _logger.LogError(
                            ex,
                            "failed to forward webhook to: {Url}, error: {Error}",
                            forward.FowardingDestination,
                            ex.Message);
                    }
                }
            }
        }

        private async Task SendNotificationsAndAlertsAsync(Webhook webhook, PlateGroup plateGroup, byte[] plateJpeg, CancellationToken cancellationToken)
        {
            if (!webhook.Group.IsPreview)
            {
                await _processorHub.Clients.All.LicensePlateRecorded(webhook.Group.BestPlateNumber);

                var alerts = await _processorContext.Alerts.ToListAsync(cancellationToken);

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
        }

        private static void MapPlateGroup(Webhook webhook, PlateGroup plateGroup)
        {
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

            MapVehicle(
                plateGroup,
                webhook);
        }

        private void ScheduleUpdateCameraOverlay(
            Webhook webhook,
            Camera camera,
            PlateGroup plateGroup,
            byte[] plateJpeg)
        {
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

                _cameraUpdateService.ScheduleOverlayRequest(updateRequest);
            }
        }

        private static void MapVehicle(
            PlateGroup plateGroup,
            Webhook webhook)
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