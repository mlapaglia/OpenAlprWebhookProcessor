using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
    {
        [ApiController]
        public class WebsocketController : ControllerBase
        {
            private readonly ILogger<WebsocketController> _logger;

            private readonly ProcessorContext _processorContext;

            private readonly WebsocketClientOrganizer _websocketClientOrganizer;

            public WebsocketController(
                ILogger<WebsocketController> logger,
                ProcessorContext processorContext,
                WebsocketClientOrganizer websocketClientOrganizer)
            {
                _logger = logger;
                _processorContext = processorContext;
                _websocketClientOrganizer = websocketClientOrganizer;
            }

            [HttpPost("/api/accountinfo")]
            public async Task<ActionResult> GetAccountInfo(CancellationToken cancellationToken)
            {
                var agent = await _processorContext.Agents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                var websocketUrl = agent.OpenAlprWebServerUrl.Replace("https://", "wss://");

                return Content(JsonSerializer.Serialize(new AccountInfoResponse()
                {
                    WebsocketsUrl = websocketUrl + "/ws",
                }));
            }

            [HttpGet("/ws")]
            public async Task GetWebsocket(CancellationToken cancellationToken)
            {
                if (HttpContext.WebSockets.IsWebSocketRequest)
                {
                    _logger.LogInformation("websocket connection received.");

                    var agent = await _processorContext.Agents
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);

                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                    var webSocketClient = new OpenAlprWebsocketClient(agent.Uid, webSocket);

                    if (_websocketClientOrganizer.AddAgent(agent.Uid, webSocketClient))
                    {
                        await webSocketClient.ConsumeMessagesAsync(cancellationToken);

                        await _websocketClientOrganizer.RemoveAgentAsync(agent.Uid, cancellationToken);
                    }
                    else
                    {
                        _logger.LogError("Unable to add websocket connection for agent: {agentId}.", agent.Uid);
                    }
                }
                else
                {
                    _logger.LogInformation("non websocket connection received.");
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
        }
    }
}
