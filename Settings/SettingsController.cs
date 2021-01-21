using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Settings.DeleteCamera;
using OpenAlprWebhookProcessor.Settings.GetAlerts;
using OpenAlprWebhookProcessor.Settings.GetCameras;
using OpenAlprWebhookProcessor.Settings.GetIgnores;
using OpenAlprWebhookProcessor.Settings.TestCamera;
using OpenAlprWebhookProcessor.Settings.UpdatedCameras;
using System;
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

        private readonly UpsertIgnoreHandler _upsertCameraHandler;

        private readonly TestCameraHandler _testCameraHandler;

        private readonly GetAgentRequestHandler _getAgentRequestHandler;

        private readonly UpsertAgentRequestHandler _upsertAgentRequestHandler;

        private readonly GetIgnoresRequestHandler _getIgnoresRequestHandler;

        private readonly GetAlertsRequestHandler _getAlertsRequestHandler;

        public SettingsController(
            GetCameraRequestHandler getCameraHandler,
            DeleteCameraHandler deleteCameraHandler,
            UpsertIgnoreHandler upsertCameraHandler,
            TestCameraHandler testCameraHandler,
            GetAgentRequestHandler getAgentRequestHandler,
            UpsertAgentRequestHandler upsertAgentRequestHandler,
            GetIgnoresRequestHandler getIgnoresRequestHandler,
            GetAlertsRequestHandler getAlertsRequestHandler)
        {
            _deleteCameraHandler = deleteCameraHandler;
            _getCameraHandler = getCameraHandler;
            _upsertCameraHandler = upsertCameraHandler;
            _testCameraHandler = testCameraHandler;
            _getAgentRequestHandler = getAgentRequestHandler;
            _upsertAgentRequestHandler = upsertAgentRequestHandler;
            _getIgnoresRequestHandler = getIgnoresRequestHandler;
            _getAlertsRequestHandler = getAlertsRequestHandler;
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

        [HttpDelete("cameras/{cameraId}")]
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

        [HttpPost("alerts")]
        public async Task UpsertAlert([FromBody] Alert alert)
        {

        }

        [HttpGet("alerts")]
        public async Task<List<Alert>> GetAlerts()
        {
            return await _getAlertsRequestHandler.HandleAsync();
        }

        [HttpDelete("alerts/{alertId}")]
        public async Task DeleteAlert(Guid alert)
        {

        }

        [HttpPost("ignores")]
        public async Task UpsertIgnore([FromBody] Ignore ignore)
        {

        }

        [HttpGet("ignores")]
        public async Task<List<Ignore>> GetIgnores()
        {
            return await _getIgnoresRequestHandler.HandleAsync();
        }

        [HttpDelete("ignores/{ignoreId}")]
        public async Task DeleteIgnore(Guid alert)
        {

        }

    }
}
