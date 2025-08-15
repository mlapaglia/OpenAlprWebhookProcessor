using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public class SinglePlateWebhookHandler
    {
        private readonly ILogger _logger;

        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebhookForwarder _webhookForwarder;

        public SinglePlateWebhookHandler(
            ILogger<SinglePlateWebhookHandler> logger,
            ISimpleCameraScheduler simpleCameraScheduler,
            IUnitOfWork unitOfWork,
            IWebhookForwarder webhookForwarder)
        {
            _logger = logger;
            _simpleCameraScheduler = simpleCameraScheduler;
            _unitOfWork = unitOfWork;
            _webhookForwarder = webhookForwarder;
        }

        public async Task HandleWebhookAsync(
            SinglePlate webhook,
            CancellationToken cancellationToken = default)
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

                await _simpleCameraScheduler.ScheduleOverlayAsync(updateRequest, cancellationToken);
            }

            var forwards = await _unitOfWork.WebhookForwards.GetAllAsync(cancellationToken);

            foreach (var forward in forwards)
            {
                if (forward.ForwardSinglePlates)
                {
                    try
                    {
                        await _webhookForwarder.ForwardWebhookAsync(
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
}
