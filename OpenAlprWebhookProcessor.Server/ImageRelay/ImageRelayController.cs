using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay
{
    [Authorize]
    [ApiController]
    [Route("api/images")]
    public class ImageRelayController : ControllerBase
    {
        private readonly GetSnapshotHandler _getSnapshotHandler;

        private readonly ProcessorContext _processorContext;

        public ImageRelayController(
            ProcessorContext processorContext,
            GetSnapshotHandler getSnapshotHandler)
        {
            _processorContext = processorContext;
            _getSnapshotHandler = getSnapshotHandler;
        }

        [HttpGet("{imageId}")]
        public async Task<IActionResult> GetImage(
            string imageId,
            CancellationToken cancellationToken)
        {
            try
            {
                var image = await GetImageHandler.GetImageFromLocalAsync(
                    _processorContext,
                    imageId,
                    cancellationToken);

                return File(image, "image/jpeg");
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet("crop/{imageId}")]
        public async Task<IActionResult> GetCropImage(
            string imageId,
            CancellationToken cancellationToken)
        {
            try
            {
                var cropImage = await GetImageHandler.GetCropImageFromLocalAsync(
                    _processorContext,
                    imageId,
                    cancellationToken);

                return File(cropImage, "image/jpeg");
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet("{cameraId}/snapshot")]
        public async Task<IActionResult> GetSnapshot(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            try
            {
                var snapshot = await _getSnapshotHandler.GetSnapshotAsync(
                    cameraId,
                    cancellationToken);

                return File(snapshot, "image/jpeg");
            }
            catch
            {
                return NotFound();
            }
        }
    }
}