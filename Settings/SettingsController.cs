using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Settings.DeleteCamera;
using OpenAlprWebhookProcessor.Settings.GetCameras;
using OpenAlprWebhookProcessor.Settings.TestCamera;
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

        private readonly TestCameraHandler _testCameraHandler;

        private readonly GetAgentRequestHandler _getAgentRequestHandler;

        private readonly UpsertAgentRequestHandler _upsertAgentRequestHandler;

        public SettingsController(
            GetCameraRequestHandler getCameraHandler,
            DeleteCameraHandler deleteCameraHandler,
            UpsertCameraHandler upsertCameraHandler,
            TestCameraHandler testCameraHandler,
            GetAgentRequestHandler getAgentRequestHandler,
            UpsertAgentRequestHandler upsertAgentRequestHandler)
        {
            _deleteCameraHandler = deleteCameraHandler;
            _getCameraHandler = getCameraHandler;
            _upsertCameraHandler = upsertCameraHandler;
            _testCameraHandler = testCameraHandler;
            _getAgentRequestHandler = getAgentRequestHandler;
            _upsertAgentRequestHandler = upsertAgentRequestHandler;
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

        [HttpPost("cameras/{cameraId}/test")]
        public IActionResult TestCamera(long cameraId)
        {
            _testCameraHandler.SendTestCameraOverlay(cameraId);

            return Ok();
        }

        [HttpGet("agent")]
        public async Task<Agent> GetAgent()
        {
            return await _getAgentRequestHandler.HandleAsync();
        }

        [HttpPost("agent")]
        public async Task UpsertAgent([FromBody] Agent agent)
        {
            await _upsertAgentRequestHandler.HandleAsync(agent);
        }
    }
}
