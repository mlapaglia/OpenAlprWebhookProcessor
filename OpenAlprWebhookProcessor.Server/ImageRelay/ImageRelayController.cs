using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.ImageRelay.GetImage;
using OpenAlprWebhookProcessor.ImageRelay.SnapshotRelay;
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
        private readonly IMediator _mediator;

        public ImageRelayController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{imageId}")]
        public async Task<IActionResult> GetImage(
            string imageId,
            CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetImageQuery(imageId);
                var image = await _mediator.Send(query, cancellationToken);

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
                var query = new GetCropImageQuery(imageId);
                var cropImage = await _mediator.Send(query, cancellationToken);

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
                var query = new GetSnapshotQuery(cameraId);
                var snapshot = await _mediator.Send(query, cancellationToken);

                return File(snapshot, "image/jpeg");
            }
            catch
            {
                return NotFound();
            }
        }
    }
}