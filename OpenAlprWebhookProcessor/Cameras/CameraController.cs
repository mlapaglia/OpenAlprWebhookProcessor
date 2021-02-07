﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    [Authorize]
    [ApiController]
    [Route("cameras")]
    public class CameraController : Controller
    {
        private readonly GetCameraRequestHandler _getCameraHandler;

        private readonly DeleteCameraHandler _deleteCameraHandler;

        private readonly UpsertCameraHandler _upsertCameraHandler;

        private readonly TestCameraHandler _testCameraHandler;

        public CameraController(
            GetCameraRequestHandler getCameraHandler,
            DeleteCameraHandler deleteCameraHandler,
            UpsertCameraHandler upsertCameraHandler,
            TestCameraHandler testCameraHandler)
        {
            _getCameraHandler = getCameraHandler;
            _deleteCameraHandler = deleteCameraHandler;
            _upsertCameraHandler = upsertCameraHandler;
            _testCameraHandler = testCameraHandler;
        }

        [HttpGet]
        public async Task<List<Camera>> GetCameras()
        {
            return await _getCameraHandler.HandleAsync();
        }

        [HttpPost]
        public async Task UpsertCamera([FromBody] Camera camera)
        {
            await _upsertCameraHandler.UpsertCameraAsync(camera);
        }

        [HttpPost("{cameraId}/delete")]
        public async Task DeleteCamera(Guid cameraId)
        {
            await _deleteCameraHandler.HandleAsync(cameraId);
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
                CameraUpdateService.SunriseSunset.Sunset);

            return Ok();
        }

        [HttpPost("{cameraId}/test/day")]
        public IActionResult TestDayMode(Guid cameraId)
        {
            _testCameraHandler.SendNightModeCommand(
                cameraId,
                CameraUpdateService.SunriseSunset.Sunrise);

            return Ok();
        }
    }
}
