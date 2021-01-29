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
using System.Threading;
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

        private readonly GetIgnoresRequestHandler _getIgnoresRequestHandler;

        private readonly GetAlertsRequestHandler _getAlertsRequestHandler;

        private readonly UpsertIgnoresRequestHandler _upsertIgnoresRequestHandler;

        private readonly UpsertAlertsRequestHandler _upsertAlertsRequestHandler;

        public SettingsController(
            GetCameraRequestHandler getCameraHandler,
            DeleteCameraHandler deleteCameraHandler,
            UpsertCameraHandler upsertCameraHandler,
            TestCameraHandler testCameraHandler,
            GetAgentRequestHandler getAgentRequestHandler,
            UpsertAgentRequestHandler upsertAgentRequestHandler,
            GetIgnoresRequestHandler getIgnoresRequestHandler,
            GetAlertsRequestHandler getAlertsRequestHandler,
            UpsertIgnoresRequestHandler upsertIgnoresRequestHandler,
            UpsertAlertsRequestHandler upsertAlertsRequestHandler)
        {
            _deleteCameraHandler = deleteCameraHandler;
            _getCameraHandler = getCameraHandler;
            _upsertCameraHandler = upsertCameraHandler;
            _testCameraHandler = testCameraHandler;
            _getAgentRequestHandler = getAgentRequestHandler;
            _upsertAgentRequestHandler = upsertAgentRequestHandler;
            _getIgnoresRequestHandler = getIgnoresRequestHandler;
            _getAlertsRequestHandler = getAlertsRequestHandler;
            _upsertIgnoresRequestHandler = upsertIgnoresRequestHandler;
            _upsertAlertsRequestHandler = upsertAlertsRequestHandler;
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
        public async Task DeleteCamera(Guid cameraId)
        {
            await _deleteCameraHandler.HandleAsync(cameraId);
        }

        [HttpPost("cameras/{cameraId}/test")]
        public IActionResult TestCamera(Guid cameraId)
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

        [HttpPost("alerts/add")]
        public async Task AddAlert([FromBody] Alert alert)
        {
            await _upsertAlertsRequestHandler.AddAlertAsync(alert);
        }

        [HttpPost("alerts")]
        public async Task UpsertAlerts([FromBody] List<Alert> alerts)
        {
            await _upsertAlertsRequestHandler.UpsertAlertsAsync(alerts);
        }

        [HttpGet("alerts")]
        public async Task<List<Alert>> GetAlerts(CancellationToken cancellationToken)
        {
            return await _getAlertsRequestHandler.HandleAsync(cancellationToken);
        }

        [HttpPost("ignores/add")]
        public async Task AddIgnore([FromBody] Ignore ignore)
        {
            await _upsertIgnoresRequestHandler.AddIgnoreAsync(ignore);
        }

        [HttpPost("ignores")]
        public async Task UpsertIgnore([FromBody] List<Ignore> ignores)
        {
            await _upsertIgnoresRequestHandler.UpsertIgnoresAsync(ignores);
        }

        [HttpGet("ignores")]
        public async Task<List<Ignore>> GetIgnores()
        {
            return await _getIgnoresRequestHandler.HandleAsync();
        }
    }
}
