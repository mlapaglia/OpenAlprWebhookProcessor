using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.SetZoomAndFocus;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraNightMode;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraOverlay;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCamera;
using OpenAlprWebhookProcessor.Features.Cameras.Commands.UpsertCameraMask;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameras;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetPlateCaptures;
using OpenAlprWebhookProcessor.Features.Cameras.Queries.GetZoomAndFocus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace OpenAlprWebhookProcessor.Features.Cameras
{
    [Authorize]
    [ApiController]
    [Route("api/cameras")]
    public class CameraController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CameraController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<List<Camera>> GetCameras()
        {
            var query = new GetCamerasQuery();
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task UpsertCamera([FromBody] Camera camera)
        {
            var command = new UpsertCameraCommand(camera);
            await _mediator.Send(command);
        }

        [HttpPost("{CameraId}/delete")]
        public async Task DeleteCamera(Guid cameraId)
        {
            var command = new DeleteCameraCommand(cameraId);
            await _mediator.Send(command);
        }

        [HttpPost("{CameraId}/test/overlay")]
        public async Task<IActionResult> TestOverlay(Guid cameraId)
        {
            var command = new TestCameraOverlayCommand(cameraId);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPost("{CameraId}/test/night")]
        public async Task<IActionResult> TestNightMode(Guid cameraId)
        {
            var command = new TestCameraNightModeCommand(cameraId);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPost("{CameraId}/test/day")]
        public async Task<IActionResult> TestDayMode(Guid cameraId)
        {
            var command = new TestCameraDayModeCommand(cameraId);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpGet("{CameraId}/zoomAndFocus")]
        public async Task<ZoomFocus> GetZoomAndFocus(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var query = new GetZoomAndFocusQuery(cameraId);
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpPost("{CameraId}/zoomAndFocus")]
        public async Task SetZoomAndFocus(
            Guid cameraId,
            [FromBody] ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken)
        {
            var command = new SetZoomAndFocusCommand(cameraId, zoomAndFocus);
            await _mediator.Send(command, cancellationToken);
        }

        [HttpPost("{CameraId}/triggerAutofocus")]
        public async Task<bool> TriggerAutofocus(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var command = new TriggerAutofocusCommand(cameraId);
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpPost("{CameraId}/mask")]
        public async Task<bool> UpsertImageMask(
            CameraMask cameraMask,
            CancellationToken cancellationToken)
        {
            var command = new UpsertCameraMaskCommand(cameraMask);
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpGet("{CameraId}/mask/coordinates")]
        public async Task<List<MaskCoordinate>> GetImageMaskCoordinates(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var query = new GetCameraMaskQuery(cameraId);
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpGet("{CameraId}/plateCaptures")]
        public async Task<List<string>> GetPlateCaptures(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var query = new GetPlateCapturesQuery(cameraId);
            return await _mediator.Send(query, cancellationToken);
        }
    }
}
