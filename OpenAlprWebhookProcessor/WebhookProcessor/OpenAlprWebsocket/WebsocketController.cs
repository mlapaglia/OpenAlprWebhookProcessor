using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

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

            private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

            public WebsocketController(
                ILogger<WebsocketController> logger,
                IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub,
                ProcessorContext processorContext,
                WebsocketClientOrganizer websocketClientOrganizer)
            {
                _logger = logger;
                _processorContext = processorContext;
                _websocketClientOrganizer = websocketClientOrganizer;
                _processorHub = processorHub;
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
                    _logger.LogInformation("Websocket connection received.");

                    var agent = await _processorContext.Agents
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);

                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                    var webSocketClient = new OpenAlprWebsocketClient(
                        _logger,
                        agent.Uid,
                        webSocket,
                        _processorHub);

                    if (_websocketClientOrganizer.AddAgent(agent.Uid, webSocketClient))
                    {
                        _logger.LogError("Multiple websocket connections for the same agent, overwriting old connection: {agentId}.", agent.Uid);
                    }

                    await _processorHub.Clients.All.OpenAlprAgentConnected(agent.Uid, HttpContext.Connection.RemoteIpAddress.ToString());

                    try
                    {
                        await webSocketClient.ConsumeMessagesAsync(cancellationToken);

                        await _websocketClientOrganizer.RemoveAgentAsync(agent.Uid, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Websocket connection closed ungracefully.");

                        await _websocketClientOrganizer.RemoveAgentAsync(agent.Uid, cancellationToken);
                        await _processorHub.Clients.All.OpenAlprAgentDisconnected(agent.Uid, HttpContext.Connection.RemoteIpAddress.ToString());
                    }
                }
                else
                {
                    _logger.LogInformation("Non websocket connection received.");
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
        }
    }
}
