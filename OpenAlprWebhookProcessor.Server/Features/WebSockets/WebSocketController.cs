using Mediator;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetAccountInfo;
using OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetWebSocket;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebSockets
{
    /// <summary>
    /// Handles the web socket traffic to the OpenAlpr Agent. Not to be
    /// confused with <see cref="ProcessorHub.ProcessorHub"/> which
    /// communicates with the web frontend.
    /// </summary>
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WebSocketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("/api/accountinfo")]
        public async Task<ActionResult> GetAccountInfo(CancellationToken cancellationToken)
        {
            var query = new GetAccountInfoQuery();
            var response = await _mediator.Send(query, cancellationToken);

            return Content(JsonSerializer.Serialize(response));
        }

        [HttpGet("/ws")]
        public async Task GetWebsocket(CancellationToken cancellationToken)
        {
            var query = new GetWebSocketQuery(HttpContext, cancellationToken);
            await _mediator.Send(query, cancellationToken);
        }
    }
} 