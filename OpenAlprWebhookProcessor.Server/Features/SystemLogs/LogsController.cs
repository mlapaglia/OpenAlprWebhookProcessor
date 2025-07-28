using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.SystemLogs
{
    [Authorize]
    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> GetLogs(
            CancellationToken cancellationToken,
            ApiLogLevel logLevel = ApiLogLevel.Information,
            string? search = null)
        {
            var query = new GetLogsQuery(logLevel, search);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
} 