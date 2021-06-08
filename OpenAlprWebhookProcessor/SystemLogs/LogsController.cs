using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.SystemLogs
{
    [Authorize]
    [ApiController]
    [Route("licensePlates")]
    public class LogsController : ControllerBase
    {
        private readonly ILogger _logger;
        public LogsController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task GetLogs()
        {
            await Task.FromResult(true);
        }
    }
}
