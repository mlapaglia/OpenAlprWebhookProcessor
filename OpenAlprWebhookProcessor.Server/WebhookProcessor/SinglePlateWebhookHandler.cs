using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class SinglePlateWebhookHandler
    {
        private readonly ILogger _logger;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly IUnitOfWork _unitOfWork;

        public SinglePlateWebhookHandler(
            ILogger<GroupWebhookHandler> logger,
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _cameraUpdateService = cameraUpdateService;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleWebhookAsync(
            SinglePlate webhook,
            CancellationToken cancellationToken)
        {
            var camera = await _unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.OpenAlprCameraId == webhook.CameraId,
                cancellationToken);

            if (camera == null)
            {
                throw new ArgumentException("unknown camera, skipping");
            }

            if (!camera.OpenAlprEnabled)
            {
                throw new ArgumentException("camera has OpenALPR integration disabled, skipping");
            }

            if (camera.UpdateOverlayEnabled)
            {
                var updateRequest = new CameraUpdateRequest()
                {
                    LicensePlateImageUuid = webhook.Uuid,
                    LicensePlate = webhook.Results[0].Plate,
                    LicensePlateJpeg = Convert.FromBase64String(webhook.Results[0].PlateCropJpeg),
                    Id = camera.Id,
                    OpenAlprProcessingTimeMs = Math.Round(webhook.ProcessingTimeMs, 2),
                    ProcessedPlateConfidence = Math.Round(webhook.Results[0].Confidence, 2),
                    IsAlert = webhook.DataType == "alpr_alert",
                    IsSinglePlate = true,
                };

                _cameraUpdateService.ScheduleOverlayRequest(updateRequest);
            }

            var forwards = await _unitOfWork.WebhookForwards.GetAllAsync(cancellationToken);

            foreach (var forward in forwards)
            {
                if (forward.ForwardSinglePlates)
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
    }
}
