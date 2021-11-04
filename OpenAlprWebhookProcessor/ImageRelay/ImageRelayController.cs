using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay
{
    [Authorize]
    [ApiController]
    [Route("images")]
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
        public async Task<Stream> GetImage(
            string imageId,
            CancellationToken cancellationToken)
        {
            return await GetImageHandler.GetImageFromLocalAsync(
                _processorContext,
                imageId,
                cancellationToken);
        }

        [HttpGet("crop/{imageId}")]
        public async Task<Stream> GetCropImage(
            string imageId,
            CancellationToken cancellationToken)
        {
            return await GetImageHandler.GetCropImageFromLocalAsync(
                _processorContext,
                imageId,
                cancellationToken);
        }

        [HttpGet("{cameraId}/snapshot")]
        public async Task<Stream> GetSnapshot(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            return await _getSnapshotHandler.GetSnapshotAsync(
                cameraId,
                cancellationToken);
        }
    }
}
