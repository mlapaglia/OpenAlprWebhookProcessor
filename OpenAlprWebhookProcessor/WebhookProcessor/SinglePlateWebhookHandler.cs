using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class SinglePlateWebhookHandler
    {
        private readonly ILogger _logger;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        public SinglePlateWebhookHandler(
            ILogger<GroupWebhookHandler> logger,
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            ProcessorContext processorContext)
        {
            _logger = logger;
            _cameraUpdateService = cameraUpdateService;
            _processorContext = processorContext;
        }

        public async Task HandleWebhookAsync(
            SinglePlate webhook,
            CancellationToken cancellationToken)
        {
            var camera = await _processorContext.Cameras
                .Where(x => x.OpenAlprCameraId == webhook.CameraId)
                .FirstOrDefaultAsync(cancellationToken);

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
                };

                _cameraUpdateService.ScheduleOverlayRequest(updateRequest);
            }

            var forwards = await _processorContext.WebhookForwards.ToListAsync(cancellationToken);

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
