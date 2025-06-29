using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Server.Cameras.DeleteCamera;
using OpenAlprWebhookProcessor.Server.Cameras.GetCameras;
using OpenAlprWebhookProcessor.Server.Cameras.GetMask;
using OpenAlprWebhookProcessor.Server.Cameras.GetPlateCaptures;
using OpenAlprWebhookProcessor.Server.Cameras.TestCamera;
using OpenAlprWebhookProcessor.Server.Cameras.UpsertCamera;
using OpenAlprWebhookProcessor.Server.Cameras.UpsertMask;
using OpenAlprWebhookProcessor.Server.Cameras.ZoomAndFocus;
using OpenAlprWebhookProcessor.Server.CameraUpdateService;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.Cameras
{
    [Authorize]
    [ApiController]
    [Route("api/cameras")]
    public class CameraController : Controller
    {
        private readonly GetCameraRequestHandler _getCameraHandler;

        private readonly DeleteCameraHandler _deleteCameraHandler;

        private readonly UpsertCameraHandler _upsertCameraHandler;

        private readonly TestCameraHandler _testCameraHandler;

        private readonly GetZoomAndFocusHandler _getZoomAndFocusHandler;

        private readonly SetZoomAndFocusHandler _setZoomAndFocusHandler;

        private readonly TriggerAutofocusHandler _triggerAutofocusHandler;

        private readonly UpsertCameraMaskHandler _upsertCameraMaskHandler;

        private readonly GetCameraMaskHandler _getCameraMaskHandler;

        private readonly GetPlateCapturesHandler _getPlateCapturesHandler;
        public CameraController(
            GetCameraRequestHandler getCameraHandler,
            DeleteCameraHandler deleteCameraHandler,
            UpsertCameraHandler upsertCameraHandler,
            TestCameraHandler testCameraHandler,
            GetZoomAndFocusHandler getZoomAndFocusHandler,
            SetZoomAndFocusHandler setZoomAndFocusHandler,
            TriggerAutofocusHandler triggerAutofocusHandler,
            UpsertCameraMaskHandler upsertCameraMaskHandler,
            GetCameraMaskHandler getCameraMaskHandler,
            GetPlateCapturesHandler getPlateCapturesHandler)
        {
            _getCameraHandler = getCameraHandler;
            _deleteCameraHandler = deleteCameraHandler;
            _upsertCameraHandler = upsertCameraHandler;
            _testCameraHandler = testCameraHandler;
            _getZoomAndFocusHandler = getZoomAndFocusHandler;
            _setZoomAndFocusHandler = setZoomAndFocusHandler;
            _triggerAutofocusHandler = triggerAutofocusHandler;
            _upsertCameraMaskHandler = upsertCameraMaskHandler;
            _getCameraMaskHandler = getCameraMaskHandler;
            _getPlateCapturesHandler = getPlateCapturesHandler;
        }

        [HttpGet]
        public async Task<List<Camera>> GetCameras()
        {
            return await _getCameraHandler.HandleAsync();
        }

        [HttpPost]
        public async Task UpsertCamera(
            [FromBody] Camera camera,
            CancellationToken cancellationToken)
        {
            await _upsertCameraHandler.UpsertCameraAsync(
                camera,
                cancellationToken);
        }

        [HttpPost("{cameraId}/delete")]
        public async Task DeleteCamera(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            await _deleteCameraHandler.HandleAsync(
                cameraId,
                cancellationToken);
        }

        [HttpPost("{cameraId}/test/overlay")]
        public IActionResult TestOverlay(Guid cameraId)
        {
            _testCameraHandler.SendTestCameraOverlay(cameraId);

            return Ok();
        }

        [HttpPost("{cameraId}/test/night")]
        public IActionResult TestNightMode(Guid cameraId)
        {
            _testCameraHandler.SendNightModeCommand(
                cameraId,
                SunriseSunset.Sunset);

            return Ok();
        }

        [HttpPost("{cameraId}/test/day")]
        public IActionResult TestDayMode(Guid cameraId)
        {
            _testCameraHandler.SendNightModeCommand(
                cameraId,
                SunriseSunset.Sunrise);

            return Ok();
        }

        [HttpGet("{cameraId}/zoomAndFocus")]
        public async Task<ZoomFocus> GetZoomAndFocus(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            return await _getZoomAndFocusHandler.HandleAsync(
                cameraId,
                cancellationToken);
        }

        [HttpPost("{cameraId}/zoomAndFocus")]
        public async Task SetZoomAndFocus(
            Guid cameraId,
            [FromBody] ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken)
        {
            await _setZoomAndFocusHandler.HandleAsync(
                cameraId,
                zoomAndFocus,
                cancellationToken);
        }

        [HttpPost("{cameraId}/triggerAutofocus")]
        public async Task<bool> TriggerAutofocus(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            return await _triggerAutofocusHandler.HandleAsync(
                cameraId,
                cancellationToken);
        }

        [HttpPost("{cameraId}/mask")]
        public async Task<bool> UpsertImageMask(
            CameraMask cameraMask,
            CancellationToken cancellationToken)
        {
            return await _upsertCameraMaskHandler.UpsertCameraMaskAsync(
                cameraMask,
                cancellationToken);
        }

        [HttpGet("{cameraId}/mask/coordinates")]
        public async Task<List<MaskCoordinate>> GetImageMaskCoordinates(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            return await _getCameraMaskHandler.GetCameraMaskAsync(
                cameraId,
                cancellationToken);
        }

        [HttpGet("{cameraId}/plateCaptures")]
        public async Task<List<string>> GetPlateCaptures(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            return await _getPlateCapturesHandler.HandleAsync(
                cameraId,
                cancellationToken);
        }
    }
}
