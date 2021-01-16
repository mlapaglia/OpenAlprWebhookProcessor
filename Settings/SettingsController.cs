using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Settings.DeleteCamera;
using OpenAlprWebhookProcessor.Settings.GetCameras;
using OpenAlprWebhookProcessor.Settings.UpdatedCameras;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    [Authorize]
    [ApiController]
    [Route("settings")]
    public class SettingsController : ControllerBase
    {
        private readonly GetCameraRequestHandler _getCameraHandler;

        private readonly DeleteCameraHandler _deleteCameraHandler;

        private readonly UpsertCameraHandler _upsertCameraHandler;

        public SettingsController(
            GetCameraRequestHandler getCameraHandler,
            DeleteCameraHandler deleteCameraHandler,
            UpsertCameraHandler upsertCameraHandler)
        {
            _deleteCameraHandler = deleteCameraHandler;
            _getCameraHandler = getCameraHandler;
            _upsertCameraHandler = upsertCameraHandler;
        }

        [HttpGet("cameras")]
        public async Task<List<Camera>> GetCameras()
        {
            return await _getCameraHandler.HandleAsync();
        }

        [HttpPost("camera")]
        public async Task UpsertCamera([FromBody] Camera camera)
        {
            await _upsertCameraHandler.UpsertCameraAsync(camera);
        }

        [HttpPost("cameras/{cameraId}/delete")]
        public async Task CreateCamera(long cameraId)
        {
            await _deleteCameraHandler.HandleAsync(cameraId);
        }
    }
}
